using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.Tests.Boundary;
using Buddhabrot.Core.Utilities;
using CliWrap;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class BoundaryScanningProcess : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateOutputPath() => SetUpOutputPath(nameof(BoundaryScanningProcess));

	public string SessionFilePath => Path.Combine(OutputPath.FullName, "session.json");

	public sealed record ScanSession(int MaxX, int MaxY, IReadOnlyList<Step> Steps);

	public readonly record struct Step(int X, int Y, VisitedRegionType Type);

	[Test]
	public void CreateAndSaveSession()
	{
		var session = CreateSession();

		using var fs = File.Open(SessionFilePath, FileMode.Create);
		JsonSerializer.Serialize(fs, session);
	}

	[Test]
	public async Task GenerateFramesAsync()
	{
		foreach (var png in OutputPath.GetFiles("*.png"))
		{
			png.Delete();
		}

		var session = LoadSession();

		var colorLookup = new Dictionary<VisitedRegionType, SKColor>
		{
			{ VisitedRegionType.Unknown, SKColors.White },
			{ VisitedRegionType.Border, SKColors.Blue },
			{ VisitedRegionType.Filament, SKColors.Aqua },
			{ VisitedRegionType.Rejected, SKColors.Gray },
		};

		using var image = new RasterImage(session.MaxX + 1, session.MaxY + 1, scale: 2);
		image.Fill(SKColors.White);

		var fire = new[]
		{
			Color.Brown.ToSKColor(),
			Color.DarkRed.ToSKColor(),
			Color.Red.ToSKColor(),
			Color.OrangeRed.ToSKColor(),
			Color.Orange.ToSKColor(),
		};

		var buffer = new RingBuffer<Step>(fire.Length * 2);

		int frame = 0;

		SaveImage(image, $"frame{frame++:00000}");

		foreach (var batch in session.Steps.Chunk(fire.Length))
		{
			buffer.AddRange(batch);

			int index = 0;
			foreach (var step in buffer)
			{
				var color = index >= fire.Length ? fire[index - fire.Length] : colorLookup[step.Type];

				image.SetPixel(step.X, (session.MaxY - step.Y), color);

				index++;
			}

			SaveImage(image, $"frame{frame++:00000}");
		}

		foreach (var step in buffer)
		{
			var color = colorLookup[step.Type];

			image.SetPixel(step.X, (session.MaxY - step.Y), color);
		}

		SaveImage(image, $"frame{frame++:00000}");

		await Cli.Wrap("ffmpeg")
			.WithArguments(args =>
				args.Add("-framerate")
					.Add("60")
					.Add("-i")
					.Add("frame%05d.png")
					.Add("-pix_fmt")
					.Add("yuv420p")
					.Add("-y")
					.Add("output.mp4")
			)
			.WithWorkingDirectory(OutputPath.FullName)
			.ExecuteAsync();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			await Cli.Wrap("open").WithArguments("output.mp4").WithWorkingDirectory(OutputPath.FullName).ExecuteAsync();
		}
		// ffmpeg -framerate 60 -i frame%05d.png -pix_fmt yuv420p -y output.mp4 && open output.mp4
	}

	private ScanSession LoadSession()
	{
		using var fs = File.OpenRead(SessionFilePath);
		return JsonSerializer.Deserialize<ScanSession>(fs) ?? throw new InvalidDataException();
	}

	private ScanSession CreateSession()
	{
		var parameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 9), MaxIterations: 15_000_000);
		var steps = new List<Step>();
		int maxX = 0;
		int maxY = 0;
		BoundaryCalculator.VisitBoundary(
			IRegionClassifier.Create(parameters),
			visitedRegions: new VisitedRegionProxy(
				new HashSetListVisitedRegions(parameters.Divisions.QuadrantDivisions),
				(regionId, type) =>
				{
					maxX = Math.Max(maxX, regionId.X);
					maxY = Math.Max(maxY, regionId.Y);
					steps.Add(new Step(regionId.X, regionId.Y, type));
				}
			)
		);

		return new ScanSession(maxX, maxY, steps);
	}

	private sealed class VisitedRegionProxy : IVisitedRegions
	{
		private readonly IVisitedRegions _real;
		private readonly Action<RegionId, VisitedRegionType> _logVisit;

		public VisitedRegionProxy(IVisitedRegions real, Action<RegionId, VisitedRegionType> logVisit)
		{
			_real = real;
			_logVisit = logVisit;
		}

		public void Visit(RegionId id, VisitedRegionType type)
		{
			_logVisit(id, type);

			_real.Visit(id, type);
		}

		public bool HasVisited(RegionId id) => _real.HasVisited(id);
	}
}
