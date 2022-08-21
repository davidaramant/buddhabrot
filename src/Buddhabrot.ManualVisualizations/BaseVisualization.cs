using Buddhabrot.Core.Images;
using Humanizer;

namespace Buddhabrot.ManualVisualizations;

public abstract class BaseVisualization
{
    private DirectoryInfo _path = new(".");

    protected void SetUpOutputPath(string name) =>
        _path = CreateDirectory(name.Humanize(LetterCasing.Title));
    
    protected void SaveImage(RasterImage image, string name) =>
        image.Save(Path.Combine(_path.FullName, name + ".png"));
    
    static DirectoryInfo CreateDirectory(string folderName) =>
        Directory.CreateDirectory(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Buddhabrot Visualizations",
                folderName));
}