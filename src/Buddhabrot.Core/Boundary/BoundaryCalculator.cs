using System.Diagnostics;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
	public static void VisitBoundary(
		IRegionsToCheck regionsToCheck,
		IRegionClassifier classifier,
		IVisitedRegions visitedRegions,
		CancellationToken cancelToken = default
	)
	{
		foreach (var region in regionsToCheck)
		{
			if (cancelToken.IsCancellationRequested)
				return;

			if (visitedRegions.HasVisited(region))
				continue;

			var regionType = classifier.ClassifyRegion(region);

			visitedRegions.Visit(region, regionType);

			if (regionType != VisitedRegionType.Rejected)
			{
				// We don't need to check the upper bounds - the set doesn't reach out that far
				// This means we can optimize the bound checks a bit

				regionsToCheck.Add(region.MoveUp());
				regionsToCheck.Add(region.MoveUpRight());
				regionsToCheck.Add(region.MoveRight());

				if (region.Y >= 1)
				{
					regionsToCheck.Add(region.MoveDownRight());
					regionsToCheck.Add(region.MoveDown());
				}

				if (region is { X: >= 1, Y: >= 1 })
				{
					regionsToCheck.Add(region.MoveDownLeft());
				}

				if (region.X >= 1)
				{
					regionsToCheck.Add(region.MoveLeft());
					regionsToCheck.Add(region.MoveUpLeft());
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

		RegionsToCheck leftRegionsToCheck = [boundaryParameters.Divisions.LeftBorderStart()];
		RegionsToCheck rightRegionsToCheck = [boundaryParameters.Divisions.RightBorderStart()];

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
			leftRegionsToCheck.Add(region);
		}

		VisitBoundary(leftRegionsToCheck, leftClassifier, visitedRegions, cancelToken);

		var boundaryRegions = visitedRegions.GetBorderRegions();

		var transformer = new QuadtreeCompressor(visitedRegions);
		var lookup = transformer.Transform();

		saveBorderData(boundaryParameters, boundaryRegions, lookup);

		var (leafs, leafQuads, branches) = visitedRegions.Nodes.Aggregate(
			(leafs: 0, leafQuads: 0, branches: 0),
			(counts, node) =>
				node.NodeType switch
				{
					VisitNodeType.Leaf => (counts.leafs + 1, counts.leafQuads, counts.branches),
					VisitNodeType.LeafQuad => (counts.leafs, counts.leafQuads + 1, counts.branches),
					VisitNodeType.Branch => (counts.leafs, counts.leafQuads, counts.branches + 1),
					_ => throw new ArgumentOutOfRangeException(),
				}
		);

		return new Metrics(
			Duration: timer.Elapsed,
			NumBorderRegions: boundaryRegions.Count,
			NumVisitedRegionNodes: visitedRegions.NodeCount,
			PercentVisitedRegionLeafNodes: ((double)leafs / visitedRegions.NodeCount),
			PercentVisitedRegionLeafQuadNodes: ((double)leafQuads / visitedRegions.NodeCount),
			PercentVisitedRegionBranchNodes: ((double)branches / visitedRegions.NodeCount),
			NumRegionLookupNodes: lookup.NodeCount
		);
	}
}
