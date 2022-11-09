using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Images;

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
    public void CreateSession()
    {
        var parameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 8), MaxIterations: 15_000_000);
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

        var session = new ScanSession(
            maxX,
            maxY,
            steps);

        using var fs = File.Open(SessionFilePath, FileMode.Create);
        JsonSerializer.Serialize(fs, session);
    }

    [Test]
    public void GenerateFrames()
    {
        var session = LoadSession();

        using var image = new RasterImage(session.MaxX + 1, session.MaxY + 1, scale: 4);
        image.Fill(Color.White);

        for (int frame = 0; frame < session.Steps.Count; frame++)
        {
            var step = session.Steps[frame];

            image.SetPixel(step.X, (session.MaxY - step.Y), Color.Black);
            SaveImage(image, $"frame{frame:00000}");
        }
        // ffmpeg -framerate 60 -i frame%05d.png -pix_fmt yuv420p output.mp4
    }

    private ScanSession LoadSession()
    {
        using var fs = File.OpenRead(SessionFilePath);
        return JsonSerializer.Deserialize<ScanSession>(fs) ?? throw new InvalidDataException();
    }
}