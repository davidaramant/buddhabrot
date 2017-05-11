using System;
using System.Diagnostics;
using System.Drawing;
using Buddhabrot.Core;
using Humanizer;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    static class EdgeSpanLocator
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanLocator));

        public static void FindEdgeSpans(
            string outputFilePath,
            ComplexArea logicalViewPort,
            int logicalPointResolution,
            IntRange iterationRange)
        {
            // Assumptions for this class
            if (Math.Abs(logicalViewPort.ImagRange.ExclusiveMax + logicalViewPort.ImagRange.InclusiveMin) >= double.Epsilon)
            {
                throw new ArgumentException($"The imaginary range of the viewport must be centered around the real axis.");
            }
            if (logicalPointResolution % 2 != 0)
            {
                throw new ArgumentException($"The vertical resolution of the grid must be divisible by 2.");
            }

            // Divide the view port / resolution in half since the set is symmetrical across the real axis

            var targetViewPort = new ComplexArea(
                realRange: logicalViewPort.RealRange,
                imagRange: new DoubleRange(0, logicalViewPort.ImagRange.ExclusiveMax));

            var targetResolution = new Size(width: logicalPointResolution, height: logicalPointResolution / 2);

            Log.Info($"Looking for edges in {targetViewPort} with a resolution of {targetResolution.Width:N0}x{targetResolution.Height:N0}");
            Log.Info($"Saving edges to: {outputFilePath}");
            Log.Info($"Iteration count: {iterationRange}");

            var timer = Stopwatch.StartNew();

            timer.Stop();
            Log.Info($"Took {timer.Elapsed.Humanize(2)} to find edge spans.");
        }
    }
}
