using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.DataStorage;
using Buddhabrot.Core.Utilities;
using Humanizer;
using Spectre.Console;

namespace BoundaryFinder;

class Program
{
	/// <summary>
	/// Computes boundary sets.
	/// </summary>
	/// <param name="power">Vertical power</param>
	/// <param name="limitMillions">Iteration limit in millions</param>
	/// <param name="note">A note for this run. Implies that the metrics will be saved to Times.csv</param>
	/// <param name="classifier">Which classifier to use.</param>
	public static async Task<int> Main(
		int power,
		double limitMillions,
		string note = "",
		ClassifierType classifier = ClassifierType.Default
	)
	{
#if DEBUG
		Console.WriteLine("WARNING - running in debug mode!");
#endif

		Console.WriteLine($"Write results to log: {!string.IsNullOrEmpty(note)}");

		var limit = (int)(limitMillions * 1_000_000);

		var metadata = classifier == ClassifierType.Default ? string.Empty : classifier.ToString();
		var boundaryParameters = new BoundaryParameters(new AreaDivisions(power), limit, metadata);

		var defaultDataSetPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			"Buddhabrot",
			"Mandelbrot Set Boundaries"
		);

		var dataProvider = new DataProvider() { DataStoragePath = defaultDataSetPath };

		await Console.Out.WriteLineAsync($"Finding boundary... (started {DateTime.Now:HH:mm:ss})");

		Metrics metrics = await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			selectedClassifier: classifier,
			saveBorderData: dataProvider.SaveBoundaryRegions,
			CancellationToken.None
		);

		var table = new Table();

		table.AddColumn("Key");
		table.AddColumn(new TableColumn("Value").RightAligned());

		table.AddRow("Vertical Power", boundaryParameters.Divisions.VerticalPower.ToString());
		table.AddRow("Divisions", boundaryParameters.Divisions.QuadrantDivisions.ToString("N0"));
		table.AddRow("Iteration Limit", boundaryParameters.MaxIterations.ToString("N0"));
		table.AddRow("Timestamp", DateTime.Now.ToString("s"));
		table.AddRow("Duration", metrics.Duration.Humanize(2));
		table.AddRow("Border regions", metrics.NumBorderRegions.ToString("N0"));
		table.AddRow($"{nameof(VisitedRegions)} nodes", metrics.NumVisitedRegionNodes.ToString("N0"));
		table.AddRow($"{nameof(RegionLookup)} nodes", metrics.NumRegionLookupNodes.ToString("N0"));
		table.AddRow("Deduplicated size", metrics.DeduplicatedSize.ToString("P0"));

		AnsiConsole.Write(table);

		if (!string.IsNullOrEmpty(note))
		{
			var csvPath = FindTimesCsv();

			var (os, cpu, ram) = ComputerDescription.GetInfo();

			await File.AppendAllTextAsync(
				csvPath,
				Environment.NewLine
					+ string.Join(
						',',
						new[]
						{
							DateTime.Now.ToString("yyyy-MM-dd"),
							DateTime.Now.ToString("HH:mm:ss"),
							metrics.Duration.Humanize(2),
							metrics.Duration.TotalSeconds.ToString("N0"),
							os,
							cpu,
							ram,
							boundaryParameters.Divisions.VerticalPower.ToString(),
							boundaryParameters.Divisions.QuadrantDivisions.ToString("N0"),
							boundaryParameters.MaxIterations.ToString("N0"),
							metrics.NumBorderRegions.ToString("N0"),
							metrics.NumVisitedRegionNodes.ToString("N0"),
							metrics.NumRegionLookupNodes.ToString("N0"),
							metrics.DeduplicatedSize.ToString("P0"),
							note,
						}.Select(EscapeCsv)
					)
			);
		}

		static string EscapeCsv(string value)
		{
			if (value.Contains('"') || value.Contains(','))
				return '"' + value.Replace("\"", new string('"', 3)) + '"';
			return value;
		}

		static string FindTimesCsv()
		{
			var path = Environment.CurrentDirectory;

			int sanityCheck = 5;
			while (!File.Exists(Path.Combine(path, "Times.csv")))
			{
				path = Path.Combine(path, "..");
				sanityCheck--;

				if (sanityCheck == 0)
					throw new Exception("Could not find Times.csv");
			}

			return Path.Combine(Path.GetFullPath(path), "Times.csv");
		}

		return 0;
	}
}
