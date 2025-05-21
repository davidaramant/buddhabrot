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
				// We don't need to check the upper bounds - the set doesn't reach out that far
				// This means we can optimize the bound checks a bit

				regionsToCheck.Enqueue(region.MoveUp());
				regionsToCheck.Enqueue(region.MoveUpRight());
				regionsToCheck.Enqueue(region.MoveRight());

				if (region.Y >= 1)
				{
					regionsToCheck.Enqueue(region.MoveDownRight());
					regionsToCheck.Enqueue(region.MoveDown());
				}

				if (region is { X: >= 1, Y: >= 1 })
				{
					regionsToCheck.Enqueue(region.MoveDownLeft());
				}

				if (region.X >= 1)
				{
					regionsToCheck.Enqueue(region.MoveLeft());
					regionsToCheck.Enqueue(region.MoveUpLeft());
				}
			}
		}
	}

	public static async Task<Metrics> CalculateBoundaryAsync(
		BoundaryParameters boundaryParameters,
		ClassifierType selectedClassifier,
		Action<BoundaryParameters, IReadOnlyList<RegionId>, RegionLookup> saveBorderData,
		CancellationToken cancelToken
	)
	{
		var timer = Stopwatch.StartNew();

		var estimatedCapacity = boundaryParameters.Divisions.QuadrantDivisions * 4;
		var visitedRegions = new VisitedRegions(capacity: estimatedCapacity);
		var proxy = new ThreadSafeVisitedRegions(visitedRegions, cancelToken);

		Queue<RegionId> leftRegionsToCheck = new([boundaryParameters.Divisions.LeftBorderStart()]);
		Queue<RegionId> rightRegionsToCheck = new([boundaryParameters.Divisions.RightBorderStart()]);

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

		var boundaryRegions = visitedRegions.GetBorderRegions();

		var transformer = new QuadtreeCompressor(visitedRegions);
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
