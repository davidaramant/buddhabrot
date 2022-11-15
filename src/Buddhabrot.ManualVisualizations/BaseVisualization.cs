using Buddhabrot.Core.Images;
using Humanizer;

namespace Buddhabrot.ManualVisualizations;

public abstract class BaseVisualization
{
    protected DirectoryInfo OutputPath { get; private set; } = new(".");

    protected void SetUpOutputPath(string name) =>
        OutputPath = CreateDirectory(name.Humanize(LetterCasing.Title));

    protected void SaveImage(RasterImage image, string name) =>
        image.Save(Path.Combine(OutputPath.FullName, name + ".png"));

    private static DirectoryInfo CreateDirectory(string folderName) =>
        DataLocation.CreateDirectory("Visualizations", folderName);
}