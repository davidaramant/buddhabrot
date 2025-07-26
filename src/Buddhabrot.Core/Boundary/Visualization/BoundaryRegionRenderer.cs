using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public sealed record RenderingArgs(
	RenderInstructions Instructions,
	RegionLookup Lookup,
	IBoundaryPalette Palette,
	bool RenderInteriors,
	int MinIterations,
	int MaxIterations
)
{
	public int Width => Instructions.Size.Width;
	public int Height => Instructions.Size.Height;
}

public static class BoundaryRegionRenderer
{
	public static void DrawRegions(
		RenderingArgs args,
		SKCanvas canvas,
		SKBitmap previousFrame,
		CancellationToken cancelToken = default
	)
	{
		var areasToDraw = new List<RegionArea>();

		canvas.DrawRect(0, 0, args.Width, args.Height, new SKPaint { Color = args.Palette.Background });

		var center = args.Instructions.QuadtreeViewport.Center;
		var radius = args.Instructions.QuadtreeViewport.QuadrantLength;

		canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = args.Palette.InsideCircle });

		if (args.Instructions.PasteFrontBuffer)
		{
			canvas.DrawBitmap(previousFrame, source: args.Instructions.SourceRect, dest: args.Instructions.DestRect);
		}

		args.Lookup.GetVisibleAreas(
			args.Instructions.QuadtreeViewport,
			args.Instructions.GetDirtyRectangles(),
			areasToDraw
		);

		using var paint = new SKPaint();

		if (args.RenderInteriors)
		{
			DrawRegionInteriors(
				canvas,
				args.Instructions.ComplexViewport,
				args.Palette,
				args.MaxIterations,
				args.MinIterations,
				areasToDraw,
				cancelToken
			);
		}
		else
		{
			DrawRegionAreas(canvas, args.Palette, areasToDraw, cancelToken);
		}
	}

	public static void DrawRegionAreas(
		SKCanvas canvas,
		IBoundaryPalette palette,
		IEnumerable<RegionArea> areasToDraw,
		CancellationToken cancelToken = default
	)
	{
		using var paint = new SKPaint();

		foreach (var (area, type) in areasToDraw)
		{
			if (cancelToken.IsCancellationRequested)
				break;

			paint.Color = palette[type];

			canvas.DrawRect(area, paint);
		}
	}

	public static void DrawRegionInteriors(
		SKCanvas canvas,
		ComplexViewport viewPort,
		IBoundaryPalette palette,
		int maxIterations,
		int minIterations,
		List<RegionArea> areasToDraw,
		CancellationToken cancelToken = default
	)
	{
		using var paint = new SKPaint();

		var (positionsToRender, types) = GetPositionsAndTypes(areasToDraw);

		var numPoints = positionsToRender.Count;
		var points = ArrayPool<Complex>.Shared.Rent(numPoints);
		var escapeTimes = ArrayPool<EscapeTime>.Shared.Rent(numPoints);
		var distances = ArrayPool<double>.Shared.Rent(numPoints);

		try
		{
			for (int i = 0; i < numPoints; i++)
			{
				points[i] = viewPort.GetComplex(positionsToRender[i]);
			}

			VectorKernel.FindDistances(points, escapeTimes, distances, numPoints, maxIterations, cancelToken);

			for (int i = 0; i < numPoints; i++)
			{
				var time = escapeTimes[i];
				var distance = distances[i];
				var classification = (time, distance) switch
				{
					({ IsInfinite: true }, _) => PointClassification.InSet,
					var (_, d) when d < viewPort.HalfPixelWidth => PointClassification.InSet,
					var (t, _) when t.Iterations > minIterations => PointClassification.InRange,
					_ => PointClassification.OutsideSet,
				};
				var type = types.GetNextType();

				paint.Color = palette[type, classification];

				canvas.DrawPoint(positionsToRender[i].X, positionsToRender[i].Y, paint);
			}
		}
		finally
		{
			ArrayPool<Complex>.Shared.Return(points);
			ArrayPool<EscapeTime>.Shared.Return(escapeTimes);
			ArrayPool<double>.Shared.Return(distances);
		}

		return;

		static (List<SKPointI> PositionsToRender, LookupRegionTypeList Types) GetPositionsAndTypes(
			List<RegionArea> areasToDraw
		)
		{
			areasToDraw.Sort((t1, t2) => t1.Type.CompareTo(t2.Type));

			var positionsToRender = new List<SKPointI>();
			var types = new LookupRegionTypeList();

			foreach (var (area, type) in areasToDraw)
			{
				positionsToRender.AddRange(area.GetAllPositions().Select(p => new SKPointI(p.X, p.Y)));
				types.Add(type, area.GetArea());
			}

			return (positionsToRender, types);
		}
	}
}
