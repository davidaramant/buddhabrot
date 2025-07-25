using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Visualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using SkiaSharp;

namespace BoundaryExplorer.Views;

public sealed class MandelbrotRenderer : Control
{
	private ILogger _log;
	private bool _isPanning;
	private Point _panningStartPoint;
	private PositionOffset _panningOffset;
	private bool _inspectMode;

	private IRegionClassifier _regionClassifier = new CornerFirstRegionClassifier(
		new BoundaryParameters(new AreaDivisions(1), 1)
	);

	private enum RenderState
	{
		Rendering,
		Idle,
	}

	private RenderState _state = RenderState.Idle;
	private readonly Lock _stateLock = new();
	private RenderingArgs? _currentFrameArgs;
	private RenderingArgs? _nextFrameArgs;
	private CancellationTokenSource _cancelSource = new();
	private Task _renderingTask = Task.CompletedTask;

	private SKBitmap _frontBuffer = new(new SKImageInfo(10, 10));
	private SKBitmap _backBuffer = new(new SKImageInfo(10, 10));

	private SKSizeI PixelBounds => new(Math.Max(1, (int)Bounds.Width), Math.Max(1, (int)Bounds.Height));

	public static readonly StyledProperty<RenderInstructions> InstructionsProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		RenderInstructions
	>(nameof(Instructions), defaultValue: RenderInstructions.Nothing);

	public RenderInstructions Instructions
	{
		get => GetValue(InstructionsProperty);
		set => SetValue(InstructionsProperty, value);
	}

	public static readonly StyledProperty<IBoundaryPalette> PaletteProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		IBoundaryPalette
	>(nameof(Palette), defaultValue: BluePalette.Instance);

	public IBoundaryPalette Palette
	{
		get => GetValue(PaletteProperty);
		set => SetValue(PaletteProperty, value);
	}

	public static readonly StyledProperty<RegionLookup> LookupProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		RegionLookup
	>(nameof(Lookup), defaultValue: RegionLookup.Empty);

	public RegionLookup Lookup
	{
		get => GetValue(LookupProperty);
		set => SetValue(LookupProperty, value);
	}

	public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<MandelbrotRenderer, bool>(
		nameof(IsBusy)
	);

	public bool IsBusy
	{
		get => GetValue(IsBusyProperty);
		set => SetValue(IsBusyProperty, value);
	}

	public static readonly StyledProperty<RegionRenderStyle> RegionRenderStyleProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		RegionRenderStyle
	>(nameof(RegionRenderStyle), defaultValue: RegionRenderStyle.Empty);

	public RegionRenderStyle RegionRenderStyle
	{
		get => GetValue(RegionRenderStyleProperty);
		set => SetValue(RegionRenderStyleProperty, value);
	}

	public IReadOnlyCollection<RegionRenderStyle> RegionRenderStyles { get; } =
		[RegionRenderStyle.Empty, RegionRenderStyle.ExactSet, RegionRenderStyle.CloseToSet];

	public static readonly StyledProperty<int> MaximumIterationsProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		int
	>(nameof(MaximumIterations));

	public int MaximumIterations
	{
		get => GetValue(MaximumIterationsProperty);
		set => SetValue(MaximumIterationsProperty, value);
	}

	public static readonly StyledProperty<int> MinimumIterationsProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		int
	>(nameof(MinimumIterations));

	public int MinimumIterations
	{
		get => GetValue(MinimumIterationsProperty);
		set => SetValue(MinimumIterationsProperty, value);
	}

	public static readonly StyledProperty<RegionId> CursorRegionProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		RegionId
	>(nameof(CursorRegion), defaultValue: new RegionId(0, 0));

	public RegionId CursorRegion
	{
		get => GetValue(CursorRegionProperty);
		set => SetValue(CursorRegionProperty, value);
	}

	public static readonly StyledProperty<string> InspectionResultsProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		string
	>(nameof(InspectionResults), defaultValue: string.Empty);

	public string InspectionResults
	{
		get => GetValue(InspectionResultsProperty);
		set => SetValue(InspectionResultsProperty, value);
	}

	public ReactiveCommand<Unit, Unit> ResetViewCommand { get; }
	public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
	public ReactiveCommand<Unit, Unit> ToggleInspectModeCommand { get; }

	public IReadOnlyCollection<ClassifierType> ClassifierTypes { get; } = Enum.GetValues<ClassifierType>();

	public static readonly StyledProperty<ClassifierType> SelectedClassifierProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		ClassifierType
	>(nameof(SelectedClassifier), defaultValue: ClassifierType.CornerFirst);

	public ClassifierType SelectedClassifier
	{
		get => GetValue(SelectedClassifierProperty);
		set => SetValue(SelectedClassifierProperty, value);
	}

	public MandelbrotRenderer()
	{
		_log = Program.ServiceProvider.GetRequiredService<ILogger<MandelbrotRenderer>>();

		foreach (var buffer in new[] { _frontBuffer, _backBuffer })
		{
			using var canvas = new SKCanvas(buffer);
			canvas.Clear(SKColors.White);
		}

		ClipToBounds = true;

		this.WhenAnyValue(x => x.Lookup).Where(lookup => lookup.NodeCount > 1).Subscribe(_ => ResetLogicalArea());

		this.WhenAnyValue(x => x.RegionRenderStyle)
			.Skip(1) // Skip initial value
			.Subscribe(_ => HandleRenderRequest(Instructions.Everything(PixelBounds)));

		EffectiveViewportChanged += (_, _) => HandleRenderRequest(Instructions.Resize(PixelBounds));
		PointerMoved += (_, e) =>
		{
			var point = e.GetPosition(this);
			var v = _currentFrameArgs?.Instructions.ComplexViewport;
			if (v != null)
			{
				var cursorPosition = v.GetComplex(new SKPointI((int)point.X, (int)point.Y));
				var r = cursorPosition.Real + 2;
				var i = Math.Abs(cursorPosition.Imaginary);

				if (r is >= 0 and < 4 && i < 2 && Lookup.Height > 2)
				{
					var side = new AreaDivisions(Lookup.Height - 2).RegionSideLength;

					int ToInt(double l) => (int)(l / side);

					CursorRegion = new RegionId(ToInt(r), ToInt(i));
				}
			}
		};
		PointerPressed += (_, e) =>
		{
			var properties = e.GetCurrentPoint(this).Properties;

			if (properties.IsLeftButtonPressed)
			{
				if (e.ClickCount == 1)
				{
					_isPanning = true;
					_panningStartPoint = e.GetPosition(this);
				}
				else if (e.ClickCount == 2)
				{
					_isPanning = false;
					if (Instructions.QuadtreeViewport.Scale < 31)
					{
						var pos = e.GetPosition(this);
						HandleRenderRequest(Instructions.ZoomIn((int)pos.X, (int)pos.Y));
					}
				}
			}
			else if (!_inspectMode && properties.IsRightButtonPressed && e.ClickCount == 2)
			{
				HandleRenderRequest(Instructions.ZoomOut());
			}
			else if (_inspectMode && properties.IsRightButtonPressed)
			{
				var (type, description) = _regionClassifier.DescribeRegion(CursorRegion);

				InspectionResults = $"({CursorRegion.X:N0}, {CursorRegion.Y:N0}) = " + description + $"=> {type}";
			}
		};
		PointerReleased += (_, _) =>
		{
			if (_isPanning)
			{
				_isPanning = false;
				HandleRenderRequest(Instructions.Move(_panningOffset));
			}
		};
		PointerCaptureLost += (_, _) =>
		{
			if (_isPanning)
			{
				_isPanning = false;

				HandleRenderRequest(Instructions.Move(_panningOffset));
			}
		};

		ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
		ZoomOutCommand = ReactiveCommand.Create(() => HandleRenderRequest(Instructions.ZoomOut()));
		ToggleInspectModeCommand = ReactiveCommand.Create(() =>
		{
			_inspectMode = !_inspectMode;
		});
		this.WhenAnyValue(x => x.Palette)
			.Select(_ =>
			{
				HandleRenderRequest(RenderInstructions.Reset(PixelBounds));
				return Unit.Default;
			})
			.Subscribe();
		// HACK: MaxIterations is set after Lookup
		this.WhenAnyValue(x => x.MaximumIterations)
			.Select(_ =>
			{
				RecreateRegionClassifier();
				return Unit.Default;
			})
			.Subscribe();
		this.WhenAnyValue(x => x.SelectedClassifier)
			.Select(_ =>
			{
				RecreateRegionClassifier();
				return Unit.Default;
			})
			.Subscribe();
	}

	private void RecreateRegionClassifier()
	{
		_regionClassifier = IRegionClassifier.Create(
			new BoundaryParameters(new AreaDivisions(Lookup.Height - 2), MaximumIterations),
			SelectedClassifier
		);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		if (_isPanning)
		{
			var currentPos = e.GetPosition(this);
			var deltaX = (int)(currentPos.X - _panningStartPoint.X);
			var deltaY = (int)(currentPos.Y - _panningStartPoint.Y);

			_panningOffset = new PositionOffset(deltaX, deltaY);
			InvalidateVisual();
		}

		base.OnPointerMoved(e);
	}

	public override void Render(DrawingContext context) =>
		context.Custom(
			new DrawSkBitmapOperation(
				_frontBuffer,
				SKRect.Create(
					x: _panningOffset.X,
					y: _panningOffset.Y,
					width: _frontBuffer.Width,
					height: _frontBuffer.Height
				)
			)
		);

	sealed class DrawSkBitmapOperation(SKBitmap bitmap, SKRect bounds) : ICustomDrawOperation
	{
		public Rect Bounds { get; } = new(x: bounds.Left, y: bounds.Top, width: bounds.Width, height: bounds.Height);

		public void Dispose() { }

		public bool HitTest(Point p) => true;

		public bool Equals(ICustomDrawOperation? other) => false;

		public void Render(ImmediateDrawingContext context)
		{
			if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is ISkiaSharpApiLeaseFeature leaseFeature)
			{
				using var lease = leaseFeature.Lease();
				var canvas = lease.SkCanvas;
				canvas.DrawBitmap(bitmap, bounds);
			}
		}
	}

	private Task RenderToBufferAsync(RenderingArgs args, CancellationToken cancelToken)
	{
		if (_backBuffer.Width != args.Width || _backBuffer.Height != args.Height)
		{
			_backBuffer.Dispose();
			_backBuffer = new SKBitmap(new SKImageInfo(args.Width, args.Height));
		}

		using (var canvas = new SKCanvas(_backBuffer))
		{
			BoundaryRegionRenderer.DrawRegions(
				args,
				canvas: canvas,
				previousFrame: _frontBuffer,
				cancelToken: cancelToken
			);
		}

		(_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);
		_panningOffset = new();

		return DoneRenderingAsync();
	}

	private void ResetLogicalArea() => HandleRenderRequest(RenderInstructions.Reset(PixelBounds));

	protected override Size MeasureOverride(Size availableSize) => availableSize;

	#region State Machine

	private readonly Queue<RenderInstructions> _renderRequests = new();

	private void HandleRenderRequest(RenderInstructions instructions)
	{
		_renderRequests.Enqueue(instructions);
		if (_renderRequests.Count != 1)
			return;

		while (_renderRequests.Count > 0)
		{
			var inst = _renderRequests.Dequeue();

			Instructions = inst;

			var args = new RenderingArgs(
				inst,
				Lookup,
				Palette,
				RegionRenderStyle,
				MinimumIterations,
				MaximumIterations
			);

			// Skip stuff that's the same
			if (args == _currentFrameArgs)
			{
				continue;
			}

			lock (_stateLock)
			{
				switch (_state)
				{
					case RenderState.Idle:
						_state = RenderState.Rendering;
						_currentFrameArgs = args;
						StartBackgroundRendering(args);
						IsBusy = true;

						break;

					case RenderState.Rendering:
						if (args != _currentFrameArgs)
						{
							_nextFrameArgs = args;
						}
						break;
				}
			}
		}
	}

	private async Task DoneRenderingAsync()
	{
		await Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
		bool turnOffBusy = false;
		bool renderNextFrame = false;
		lock (_stateLock)
		{
			_currentFrameArgs = null;

			if (_nextFrameArgs != null)
			{
				_currentFrameArgs = _nextFrameArgs;
				renderNextFrame = true;
				_nextFrameArgs = null;
			}
			else
			{
				_state = RenderState.Idle;
				turnOffBusy = true;
			}
		}

		if (renderNextFrame)
		{
			await RenderToBufferAsync(_currentFrameArgs!, _cancelSource.Token);
		}

		if (turnOffBusy)
		{
			await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
		}
	}

	private void StartBackgroundRendering(RenderingArgs args) =>
		_renderingTask = Task.Run(() => RenderToBufferAsync(args, _cancelSource.Token));

	// TODO: At some point it might make sense to be able to cancel long running renders (especially with interiors)
	private async Task CancelRenderingAsync()
	{
		await _cancelSource.CancelAsync();
		await _renderingTask;
		_cancelSource = new();
	}

	#endregion
}
