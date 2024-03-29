using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using ReactiveUI;
using SkiaSharp;
using Vector = Avalonia.Vector;

namespace BoundaryExplorer.Views;

public sealed class MandelbrotRenderer : Control
{
	private bool _isPanning;
	private Point _panningStartPoint;
	private SquareBoundary _panningStart;
	private PixelVector _panningOffset = new();
	private bool _inspectMode = false;

	private IRegionClassifier _regionClassifier = new CornerFirstRegionClassifier(
		new BoundaryParameters(new AreaDivisions(1), 1)
	);

	private enum RenderState
	{
		Rendering,
		Idle,
	}

	private RenderState _state = RenderState.Idle;
	private RenderingArgs? _currentFrameArgs;
	private RenderingArgs? _nextFrameArgs;
	private CancellationTokenSource _cancelSource = new();
	private Task _renderingTask = Task.CompletedTask;
	private readonly List<(System.Drawing.Rectangle Rect, LookupRegionType Type)> _areasToDraw = new();
	private ViewPort? _viewPort = null;

	private RenderTargetBitmap _frontBuffer = new(new PixelSize(1, 1));
	private RenderTargetBitmap _backBuffer = new(new PixelSize(1, 1));

	private PixelSize PixelBounds => new(Math.Max(1, (int)Bounds.Width), Math.Max(1, (int)Bounds.Height));

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

	public static readonly StyledProperty<bool> RenderInteriorsProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		bool
	>(nameof(RenderInteriors));

	public bool RenderInteriors
	{
		get => GetValue(RenderInteriorsProperty);
		set => SetValue(RenderInteriorsProperty, value);
	}

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

	public static readonly StyledProperty<SquareBoundary> SetBoundaryProperty = AvaloniaProperty.Register<
		MandelbrotRenderer,
		SquareBoundary
	>(nameof(SetBoundary));

	public SquareBoundary SetBoundary
	{
		get => GetValue(SetBoundaryProperty);
		set => SetValue(SetBoundaryProperty, value);
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
		ClipToBounds = true;
		// HACK: I'm sure there is some fancy Reactive way to do this
		this.PropertyChanged += async (s, e) =>
		{
			if (e.Property.Name == nameof(Lookup) && Lookup?.NodeCount > 1)
			{
				await ResetLogicalAreaAsync();
			}
			else if (e.Property.Name == nameof(RenderInteriors))
			{
				await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
			}
		};

		this.EffectiveViewportChanged += async (_, _) =>
		{
			await RequestRenderAsync(RenderInstructions.Resized(oldSize: _frontBuffer.PixelSize, newSize: PixelBounds));
		};
		PointerMoved += (_, e) =>
		{
			var point = e.GetPosition(this);
			var v = _viewPort;
			if (v != null)
			{
				var cursorPosition = v.GetComplex((int)point.X, (int)point.Y);
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
		PointerPressed += async (_, e) =>
		{
			var properties = e.GetCurrentPoint(this).Properties;

			if (properties.IsLeftButtonPressed)
			{
				if (e.ClickCount == 1)
				{
					_isPanning = true;
					_panningStartPoint = e.GetPosition(this);
					_panningStart = SetBoundary;
				}
				else if (e.ClickCount == 2)
				{
					_isPanning = false;
					if (SetBoundary.Scale < 31)
					{
						var pos = e.GetPosition(this);
						SetBoundary = SetBoundary.ZoomIn((int)pos.X, (int)pos.Y);
						await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
					}
				}
			}
			else if (!_inspectMode && properties.IsRightButtonPressed && e.ClickCount == 2)
			{
				SetBoundary = SetBoundary.ZoomOut(PixelBounds.Width, PixelBounds.Height);
				await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
			}
			else if (_inspectMode && properties.IsRightButtonPressed)
			{
				var (type, description) = _regionClassifier.DescribeRegion(CursorRegion);

				InspectionResults = $"({CursorRegion.X:N0}, {CursorRegion.Y:N0}) = " + description + $"=> {type}";
			}
		};
		PointerReleased += async (_, e) =>
		{
			if (_isPanning)
			{
				_isPanning = false;
				SetBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
				await RequestRenderAsync(RenderInstructions.Moved(PixelBounds, _panningOffset));
			}
		};
		PointerCaptureLost += async (_, e) =>
		{
			if (_isPanning)
			{
				_isPanning = false;

				SetBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
				await RequestRenderAsync(RenderInstructions.Moved(PixelBounds, _panningOffset));
			}
		};

		ResetViewCommand = ReactiveCommand.CreateFromTask(ResetLogicalAreaAsync);
		ZoomOutCommand = ReactiveCommand.CreateFromTask(() =>
		{
			SetBoundary = SetBoundary.ZoomOut(PixelBounds.Width, PixelBounds.Height);
			return RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
		});
		ToggleInspectModeCommand = ReactiveCommand.Create(() =>
		{
			_inspectMode = !_inspectMode;
		});
		this.WhenAnyValue(x => x.Palette)
			.SelectMany(async _ =>
			{
				await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
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

			_panningOffset = new PixelVector(deltaX, deltaY);
			InvalidateVisual();
		}

		base.OnPointerMoved(e);
	}

	public override void Render(DrawingContext context)
	{
		_viewPort = ViewPort.FromResolution(
			new System.Drawing.Size(PixelBounds.Width, PixelBounds.Height),
			SetBoundary.Center,
			2d / SetBoundary.QuadrantLength
		);

		context.DrawImage(
			_frontBuffer,
			new Rect(_panningOffset.X, _panningOffset.Y, _frontBuffer.PixelSize.Width, _frontBuffer.PixelSize.Height)
		);
	}

	sealed record RenderingArgs(
		RenderInstructions Instructions,
		SquareBoundary SetBoundary,
		RegionLookup Lookup,
		IBoundaryPalette Palette,
		bool RenderInteriors,
		int MinIterations,
		int MaxIterations
	)
	{
		public int Width => Instructions.Size.Width;
		public int Height => Instructions.Size.Height;
	}

	private Task RenderToBufferAsync(RenderingArgs args, CancellationToken cancelToken)
	{
		if (_backBuffer.PixelSize.Width != args.Width || _backBuffer.PixelSize.Height != args.Height)
		{
			_backBuffer.Dispose();
			_backBuffer = new RenderTargetBitmap(new PixelSize(args.Width, args.Height), new Vector(96, 96));
		}

		// TODO: Check for cancellation
		using (var context = _backBuffer.CreateDrawingContext(null))
		{
			var skiaContext = (ISkiaDrawingContextImpl)context;
			var canvas = skiaContext.SkCanvas;

			canvas.DrawRect(0, 0, args.Width, args.Height, new SKPaint { Color = args.Palette.Background });

			var center = args.SetBoundary.Center;
			var radius = args.SetBoundary.QuadrantLength;

			canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = args.Palette.InsideCircle });

			if (args.Instructions.PasteFrontBuffer)
			{
				context.DrawBitmap(
					_frontBuffer.PlatformImpl,
					opacity: 1,
					sourceRect: args.Instructions.SourceRect,
					destRect: args.Instructions.DestRect
				);
			}

			_areasToDraw.Clear();
			args.Lookup.GetVisibleAreas(args.SetBoundary, args.Instructions.GetDirtyRectangles(), _areasToDraw);

			using var paint = new SKPaint();

			if (args.RenderInteriors)
			{
				var viewPort = ViewPort.FromResolution(
					new System.Drawing.Size(args.Width, args.Height),
					args.SetBoundary.Center,
					2d / args.SetBoundary.QuadrantLength
				);
				_areasToDraw.Sort((t1, t2) => t1.Type.CompareTo(t2.Type));

				var positionsToRender = new List<System.Drawing.Point>();
				var types = new LookupRegionTypeList();

				foreach (var (area, type) in _areasToDraw)
				{
					positionsToRender.AddRange(area.GetAllPositions());
					types.Add(type, area.GetArea());
				}

				var numPoints = positionsToRender.Count;
				var points = ArrayPool<Complex>.Shared.Rent(numPoints);
				var escapeTimes = ArrayPool<EscapeTime>.Shared.Rent(numPoints);

				for (int i = 0; i < numPoints; i++)
				{
					points[i] = viewPort.GetComplex(positionsToRender[i]);
				}

				// TODO: Why does this lock up the UI? It's already in a different Task, should this part be in a Task as well?
				VectorKernel.FindEscapeTimes(points, escapeTimes, numPoints, args.MaxIterations);

				for (int i = 0; i < numPoints; i++)
				{
					var time = escapeTimes[i];
					var classification = time switch
					{
						{ IsInfinite: true } => PointClassification.InSet,
						var t when t.Iterations > args.MinIterations => PointClassification.InRange,
						_ => PointClassification.OutsideSet,
					};
					var type = types.GetNextType();

					paint.Color = args.Palette[type, classification];

					canvas.DrawPoint(positionsToRender[i].X, positionsToRender[i].Y, paint);
				}

				ArrayPool<Complex>.Shared.Return(points);
				ArrayPool<EscapeTime>.Shared.Return(escapeTimes);
			}
			else
			{
				foreach (var (area, type) in _areasToDraw)
				{
					paint.Color = args.Palette[type];

					canvas.DrawRect(area.X, area.Y, area.Width, area.Height, paint);
				}
			}
		}

		(_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);
		_panningOffset = new();

		return DoneRenderingAsync();
	}

	private Task ResetLogicalAreaAsync()
	{
		SetBoundary = SquareBoundary.GetLargestCenteredSquareInside(PixelBounds.Width, PixelBounds.Height);
		return RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
	}

	protected override Size MeasureOverride(Size availableSize) => availableSize;

	#region State Machine

	private async Task RequestRenderAsync(RenderInstructions instructions)
	{
		var args = new RenderingArgs(
			instructions,
			SetBoundary,
			Lookup,
			Palette,
			RenderInteriors,
			MinimumIterations,
			MaximumIterations
		);

		switch (_state)
		{
			case RenderState.Idle:
				_currentFrameArgs = args;
				await StartRenderingAsync(args);
				_state = RenderState.Rendering;
				IsBusy = true;
				break;

			case RenderState.Rendering:
				if (args != _currentFrameArgs && args != _nextFrameArgs)
				{
					_nextFrameArgs = args;
				}

				break;
		}
	}

	private async Task DoneRenderingAsync()
	{
		InvalidateVisual();
		_currentFrameArgs = null;

		if (_nextFrameArgs != null)
		{
			_currentFrameArgs = _nextFrameArgs;
			await StartRenderingAsync(_nextFrameArgs);
			_nextFrameArgs = null;
			_state = RenderState.Rendering;
		}
		else
		{
			_state = RenderState.Idle;
			await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
		}
	}

	private Task StartRenderingAsync(RenderingArgs args)
	{
		_renderingTask = Task.Run(() => RenderToBufferAsync(args, _cancelSource.Token));
		return Task.CompletedTask;
	}

	private async Task CancelRenderingAsync()
	{
		_cancelSource.Cancel();
		await _renderingTask;
		_cancelSource = new();
	}

	#endregion
}
