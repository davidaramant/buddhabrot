using System.Drawing;
using System.Linq;
using System.Text.Json;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class BoundaryScanningProcess : BaseVisualization
{
    [OneTimeSetUp]
    public void CreateOutputPath() => SetUpOutputPath(nameof(BoundaryScanningProcess));

    public string SessionFilePath => Path.Combine(OutputPath.FullName, "session.json");

    public sealed record ScanSession(
        int MaxX,
        int MaxY,
        IReadOnlyList<Step> Steps);

    public readonly record struct Step(int X, int Y, RegionType Type);

    [Test]
    public void CreateAndSaveSession()
    {
        var session = CreateSession();

        using var fs = File.Open(SessionFilePath, FileMode.Create);
        JsonSerializer.Serialize(fs, session);
    }

    [Test]
    public void GenerateFrames()
    {
        foreach (var png in OutputPath.GetFiles("*.png"))
        {
            png.Delete();
        }

        var session = CreateSession();

        var palette = new Dictionary<RegionType, Color>
        {
            { RegionType.Empty, Color.LightGray },
            { RegionType.Border, Color.DarkSlateGray },
            { RegionType.Filament, Color.Gray },
            { RegionType.InSet, Color.SlateGray },
        };

        using var image = new RasterImage(session.MaxX + 1, session.MaxY + 1, scale: 2);
        image.Fill(Color.White);

        var fire = new[]
        {
            Color.Brown,
            Color.DarkRed,
            Color.Red,
            Color.OrangeRed,
            Color.Orange,
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
                var color = index >= fire.Length ? fire[index - fire.Length] : palette[step.Type];

                image.SetPixel(step.X, (session.MaxY - step.Y), color);

                index++;
            }

            SaveImage(image, $"frame{frame++:00000}");
        }

        foreach (var step in buffer)
        {
            var color = palette[step.Type];

            image.SetPixel(step.X, (session.MaxY - step.Y), color);
        }

        SaveImage(image, $"frame{frame++:00000}");
        
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
        BoundaryCalculator.FindBoundaryAndFilaments(parameters,
            logVisitedArea: (regionId, type) =>
            {
                maxX = Math.Max(maxX, regionId.X);
                maxY = Math.Max(maxY, regionId.Y);
                steps.Add(new Step(regionId.X, regionId.Y, type));
            });

        return new ScanSession(
            maxX,
            maxY,
            steps);
    }
}