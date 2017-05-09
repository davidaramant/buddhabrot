using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Buddhabrot.Core;
using log4net;
using System.Drawing;
using System.Numerics;
using Buddhabrot.IterationKernels;
using Humanizer;

namespace Buddhabrot.Edges
{
    static class EdgeLocator
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeLocator));

        public static void FindEdges(
            string outputFilePath,
            ComplexArea viewPort,
            int gridResolution,
            IntRange iterationRange)
        {
            // Assumptions for this class
            if (Vector<double>.Count != Vector<long>.Count )
            {
                throw new ArgumentException($"This assumes a vector capacity of long and double are equal.");
            }
            if (Math.Abs(viewPort.ImagRange.ExclusiveMax + viewPort.ImagRange.InclusiveMin) >= float.Epsilon)
            {
                throw new ArgumentException($"The imaginary range of the viewport must be centered around the real axis.");
            }
            if (gridResolution % 2 != 0)
            {
                throw new ArgumentException($"The vertical resolution of the grid must be divisible by 2.");
            }

            // Divide the view port / resolution in half since the set is symmetrical across the real axis

            var targetViewPort = new ComplexArea(
                realRange: viewPort.RealRange,
                imagRange: new DoubleRange(0, viewPort.ImagRange.ExclusiveMax));

            var targetResolution = new Size(width: gridResolution, height: gridResolution / 2);

            Log.Info($"Looking for {targetResolution.Width}x{targetResolution.Height} areas in {targetViewPort}");
            Log.Info($"Saving edges to: {outputFilePath}");
            Log.Info($"Iteration count: {iterationRange}");

            var timer = Stopwatch.StartNew();

            EdgeAreas.Write(
                targetResolution,
                targetViewPort,
                GetEdgeAreas(targetViewPort, targetResolution, iterationRange),
                outputFilePath);

            timer.Stop();
            Log.Info($"Took {timer.Elapsed.Humanize(2)} to find edges.");
        }

        private static IEnumerable<EdgeArea> GetEdgeAreas(ComplexArea viewPort, Size resolution, IntRange iterationRange)
        {
            var vectorSize = Vector<double>.Count;

            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = vectorSize - 1;

            var numberOfStrips = (int)Math.Ceiling((double)resolution.Height / areasInStripColumn);

            return
                Enumerable.Range(0, numberOfStrips).
                AsParallel().
                SelectMany(stripIndex => FindEdgeAreasInStrip(viewPort, resolution, iterationRange, stripIndex));
        }

        /// <summary>
        /// Finds the edge areas in a strip of points.
        /// </summary>
        /// <param name="viewPort">The view port.</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="iterationRange">The iteration range.</param>
        /// <param name="stripIndex">Index of the strip.</param>
        /// <returns>The edge areas it found.</returns>
        /// <remarks>
        /// This iterates horizontally over a columns of points in a strip.  It keeps track of two columns of results
        /// (left and right). From the columns, it determines which areas have corners that are a mix of points inside
        /// and outside the set.
        /// 
        /// It gets a bit confusing because I interpret the grid to be defined by the points in the resolution.
        /// In other words, there are (width-1) by (height-1) areas.
        /// </remarks>
        private static IEnumerable<EdgeArea> FindEdgeAreasInStrip(
            ComplexArea viewPort,
            Size resolution,
            IntRange iterationRange,
            int stripIndex)
        {
            var vectorSize = Vector<double>.Count;

            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = vectorSize - 1;

            var areaRowsRemaining = resolution.Height - stripIndex * areasInStripColumn;
            var areasInBatch = Math.Min(areaRowsRemaining, areasInStripColumn);

            double realIncrement = viewPort.RealRange.Magnitude / (resolution.Width - 1);
            double imagIncrement = viewPort.ImagRange.Magnitude / (resolution.Height - 1);

            var reals = new double[vectorSize];
            var imags = new double[vectorSize];
            var leftColumnIsInSet = new bool[vectorSize];
            var rightColumnIsInSet = new bool[vectorSize];

            double GetRealValue(int columnIndex) => viewPort.RealRange.InclusiveMin + columnIndex * realIncrement;
            double GetImagValue(int rowIndex) => viewPort.ImagRange.InclusiveMin + (stripIndex * areasInStripColumn + rowIndex) * imagIncrement;

            void IterateRightColumn(int columnIndex)
            {
                var realValue = GetRealValue(columnIndex);

                for (int rowIndex = 0; rowIndex < vectorSize; rowIndex++)
                {
                    reals[rowIndex] = realValue;
                    imags[rowIndex] = GetImagValue(rowIndex);

                    rightColumnIsInSet[rowIndex] = MandelbulbChecker.IsInsideBulbs(reals[rowIndex], imags[rowIndex]);
                }

                if (rightColumnIsInSet.Any(definitivelyInSet => !definitivelyInSet))
                {
                    var vReals = new Vector<double>(reals);
                    var vImags = new Vector<double>(imags);

                    // Do a quick check with a low bailout to see if all the points are outside the set
                    const int quickBailout = 500;
                    var vIterations = VectorKernel.IteratePoints(vReals, vImags, quickBailout);

                    void CopyResults(long max)
                    {
                        for (int i = 0; i < vectorSize; i++)
                        {
                            rightColumnIsInSet[i] = vIterations[i] == max;
                        }
                    }

                    CopyResults(quickBailout);
                    if (rightColumnIsInSet.Any(inSet => inSet))
                    {
                        vIterations = VectorKernel.IteratePoints(vReals, vImags, iterationRange.Max);

                        CopyResults(iterationRange.Max);
                    }
                }
            }

            void SwapColumnArrays()
            {
                var temp = leftColumnIsInSet;
                leftColumnIsInSet = rightColumnIsInSet;
                rightColumnIsInSet = temp;
            }

            IterateRightColumn(columnIndex: 0);
            SwapColumnArrays();

            for (int rightColumnIndex = 1; rightColumnIndex < (resolution.Width - 1); rightColumnIndex++)
            {
                IterateRightColumn(rightColumnIndex);

                // Iterate the areas bottom to top
                for (int areaIndex = 0; areaIndex < areasInBatch; areaIndex++)
                {
                    var cornersInSet =
                        (leftColumnIsInSet[areaIndex] ? Corners.BottomLeft : Corners.None)|
                        (rightColumnIsInSet[areaIndex] ? Corners.BottomRight : Corners.None)|
                        (leftColumnIsInSet[areaIndex + 1] ? Corners.TopLeft : Corners.None) |
                        (rightColumnIsInSet[areaIndex + 1] ? Corners.TopRight : Corners.None);

                    if (cornersInSet != Corners.None && cornersInSet != Corners.All)
                    {
                        yield return new EdgeArea(
                            new Point(rightColumnIndex - 1, stripIndex * areasInStripColumn + areaIndex),
                            cornersInSet);
                    }
                }

                SwapColumnArrays();
            }
        }
    }
}
