using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public sealed record RenderingArgs(
	RenderInstructions Instructions,
	SquareBoundary SetBoundary,
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

public static class RegionRenderer
{
	// 	public static void DrawRegions2(RenderingArgs args, SKCanvas canvas, SKBitmap previousFrame)
	// 	{
	// 		var areasToDraw = new List<RegionArea>();
	//
	// 		canvas.DrawRect(0, 0, args.Width, args.Height, new SKPaint { Color = args.Palette.Background });
	//
	// 		var center = args.SetBoundary.Center;
	// 		var radius = args.SetBoundary.QuadrantLength;
	//
	// 		canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = args.Palette.InsideCircle });
	//
	// 		if (args.Instructions.PasteFrontBuffer)
	// 		{
	// 			canvas.DrawBitmap(
	// 				previousFrame,
	// 				source: args.Instructions.SourceRect.ToSKRect(),
	// 				dest: args.Instructions.DestRect.ToSKRect()
	// 			);
	// 		}
	//
	// 		args.Lookup.GetVisibleAreas(args.SetBoundary, args.Instructions.GetDirtyRectangles(), areasToDraw);
	//
	// 		using var paint = new SKPaint();
	//
	// 		if (args.RenderInteriors)
	// 		{
	// 			var viewPort = ViewPort.FromResolution(
	// 				new System.Drawing.Size(args.Width, args.Height),
	// 				args.SetBoundary.Center,
	// 				2d / args.SetBoundary.QuadrantLength
	// 			);
	// 			areasToDraw.Sort((t1, t2) => t1.Type.CompareTo(t2.Type));
	//
	// 			var positionsToRender = new List<System.Drawing.Point>();
	// 			var types = new LookupRegionTypeList();
	//
	// 			foreach (var (area, type) in areasToDraw)
	// 			{
	// 				positionsToRender.AddRange(area.GetAllPositions());
	// 				types.Add(type, area.GetArea());
	// 			}
	//
	// 			var numPoints = positionsToRender.Count;
	// 			var points = ArrayPool<Complex>.Shared.Rent(numPoints);
	// 			var escapeTimes = ArrayPool<EscapeTime>.Shared.Rent(numPoints);
	//
	// 			for (int i = 0; i < numPoints; i++)
	// 			{
	// 				points[i] = viewPort.GetComplex(positionsToRender[i]);
	// 			}
	//
	// 			// TODO: Why does this lock up the UI? It's already in a different Task, should this part be in a Task as well?
	// 			VectorKernel.FindEscapeTimes(points, escapeTimes, numPoints, args.MaxIterations);
	//
	// 			for (int i = 0; i < numPoints; i++)
	// 			{
	// 				var time = escapeTimes[i];
	// 				var classification = time switch
	// 				{
	// 					{ IsInfinite: true } => PointClassification.InSet,
	// 					var t when t.Iterations > args.MinIterations => PointClassification.InRange,
	// 					_ => PointClassification.OutsideSet,
	// 				};
	// 				var type = types.GetNextType();
	//
	// 				paint.Color = args.Palette[type, classification];
	//
	// 				canvas.DrawPoint(positionsToRender[i].X, positionsToRender[i].Y, paint);
	// 			}
	//
	// 			ArrayPool<Complex>.Shared.Return(points);
	// 			ArrayPool<EscapeTime>.Shared.Return(escapeTimes);
	// 		}
	// 		else
	// 		{
	// 			foreach (var (area, type) in areasToDraw)
	// 			{
	// 				paint.Color = args.Palette[type];
	//
	// 				canvas.DrawRect(area.X, area.Y, area.Width, area.Height, paint);
	// 			}
	// 		}
	// 	}

	public static void DrawRegions(SKCanvas canvas, IBoundaryPalette palette, IEnumerable<RegionArea> areasToDraw)
	{
		using var paint = new SKPaint();

		foreach (var (area, type) in areasToDraw)
		{
			paint.Color = palette[type];

			canvas.DrawRect(area.X, area.Y, area.Width, area.Height, paint);
		}
	}
}
