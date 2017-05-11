﻿using System.Collections.Generic;
using Buddhabrot.Core;
using Buddhabrot.PointGrids;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    static class EdgeSpanLocator
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanLocator));

        public static void FindEdgeSpans(
            string pointGridFilePath,
            string outputFilePath)
        {
            using (var pointGrid = PointGrid.Load(pointGridFilePath))
            {
                EdgeSpanStream.Write(
                    outputFilePath,
                    pointGrid.PointResolution,
                    pointGrid.ViewPort,
                    GetEdgeSpans(pointGrid));
            }
        }

        private static IEnumerable<LogicalEdgeSpan> GetEdgeSpans(IEnumerable<PointRow> rows)
        {
            foreach (var (belowRow, currentRow, aboveRow) in GetRowBatches(rows))
            {
                var y = currentRow.Y;
                int maxX = currentRow.Width - 1;
                foreach (var x in currentRow.GetXPositionsOfSet())
                {
                    if (aboveRow != null)
                    {
                        if (x > 0 && !aboveRow[x - 1])
                            yield return new LogicalEdgeSpan(x, y, Direction.UpLeft);
                        if (!aboveRow[x])
                            yield return new LogicalEdgeSpan(x, y, Direction.Up);
                        if (x < maxX && !aboveRow[x + 1])
                            yield return new LogicalEdgeSpan(x, y, Direction.UpRight);
                    }

                    if (x > 0 && !currentRow[x - 1])
                        yield return new LogicalEdgeSpan(x, y, Direction.Left);
                    if (x < maxX && !currentRow[x + 1])
                        yield return new LogicalEdgeSpan(x, y, Direction.Right);

                    if (belowRow != null)
                    {
                        if (x > 0 && !belowRow[x - 1])
                            yield return new LogicalEdgeSpan(x, y, Direction.DownLeft);
                        if (!belowRow[x])
                            yield return new LogicalEdgeSpan(x, y, Direction.Down);
                        if (x < maxX && !belowRow[x + 1])
                            yield return new LogicalEdgeSpan(x, y, Direction.DownRight);
                    }
                }
            }
        }

        private static IEnumerable<(PointRow below, PointRow current, PointRow above)> GetRowBatches(
            IEnumerable<PointRow> rows)
        {
            PointRow row1 = null;
            PointRow row0 = null;

            foreach (var row in rows)
            {
                var row2 = row1;
                row1 = row0;
                row0 = row;

                if (row1 != null)
                {
                    yield return (row2, row1, row0);
                }
            }

            yield return (row1, row0, null);
        }
    }
}
