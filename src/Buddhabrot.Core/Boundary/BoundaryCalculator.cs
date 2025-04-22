using System.Diagnostics;
using Buddhabrot.Core.Boundary.Classifiers;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
	public static void VisitBoundary(
		Queue<RegionId> regionsToCheck,
		IRegionClassifier classifier,
		IVisitedRegions visitedRegions,
		CancellationToken cancelToken = default
	)
	{
		while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
		{
			var region = regionsToCheck.Dequeue();

			if (visitedRegions.HasVisited(region))
				continue;

			var regionType = classifier.ClassifyRegion(region);

			visitedRegions.Visit(region, regionType);

			if (regionType != VisitedRegionType.Rejected)
			{
				AddRegionToCheck(region.MoveUp());
				AddRegionToCheck(region.MoveUpRight());
				AddRegionToCheck(region.MoveRight());
				AddRegionToCheck(region.MoveDownRight());
				AddRegionToCheck(region.MoveDown());
				AddRegionToCheck(region.MoveDownLeft());
				AddRegionToCheck(region.MoveLeft());
				AddRegionToCheck(region.MoveUpLeft());
			}
		}

		void AddRegionToCheck(RegionId region)
		{
			// We don't need to check the upper bounds - the set doesn't reach out that far
			if (region is { X: >= 0, Y: >= 0 })
			{
				regionsToCheck.Enqueue(region);
			}
		}
	}

	public static async Task<Metrics> CalculateBoundaryAsync(
		AreaDivisions areaDivisions,
		int maximumIterations,
		string metadata,
		ClassifierType selectedClassifier,
		Action<BoundaryParameters, IReadOnlyList<RegionId>, RegionLookup> saveBorderData,
		CancellationToken cancelToken
	)
	{
		var timer = Stopwatch.StartNew();
		var boundaryParameters = new BoundaryParameters(areaDivisions, maximumIterations, metadata);

		var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);
		var proxy = new ThreadSafeVisitedRegions(visitedRegions, cancelToken);

		Queue<RegionId> leftRegionsToCheck = new([areaDivisions.LeftStart()]);
		Queue<RegionId> rightRegionsToCheck = new([areaDivisions.RightStart()]);

		var leftClassifier = IRegionClassifier.Create(boundaryParameters, selectedClassifier);
		var rightClassifier = IRegionClassifier.Create(boundaryParameters, selectedClassifier);

		await Task.WhenAll(
			Task.Factory.StartNew(
				() => VisitBoundary(leftRegionsToCheck, leftClassifier, proxy, proxy.Token),
				cancelToken,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			),
			Task.Factory.StartNew(
				() => VisitBoundary(rightRegionsToCheck, rightClassifier, proxy, proxy.Token),
				cancelToken,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			)
		);

		foreach (var region in rightRegionsToCheck)
		{
			leftRegionsToCheck.Enqueue(region);
		}

		VisitBoundary(leftRegionsToCheck, leftClassifier, visitedRegions, cancelToken);

		var boundaryRegions = visitedRegions.GetBorderRegions().ToList();

		var transformer = new QuadTreeCompressor(visitedRegions);
		var lookup = transformer.Transform();

		saveBorderData(boundaryParameters, boundaryRegions, lookup);

		return new Metrics(
			Duration: timer.Elapsed,
			NumBorderRegions: boundaryRegions.Count,
			NumVisitedRegionNodes: visitedRegions.NodeCount,
			NumRegionLookupNodes: lookup.NodeCount
		);
	}
}
