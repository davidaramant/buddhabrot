namespace Buddhabrot.ManualVisualizations;

public static class DataLocation
{
	public static DirectoryInfo CreateDirectory(params string[] folderNames) =>
		Directory.CreateDirectory(
			Path.Combine(
				new[] { Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Buddhabrot" }
					.Concat(folderNames)
					.ToArray()
			)
		);
}
