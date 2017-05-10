﻿using System;
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
            if (Vector<double>.Count != Vector<long>.Count)
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

        private static IEnumerable<EdgeArea> GetEdgeAreas(ComplexArea viewPort, Size gridResolution, IntRange iterationRange)
        {
            var cornerCalculator = new CornerCalculator(gridResolution, viewPort);

            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = VectorKernel.VectorCapacity - 1;

            var numberOfStrips = (int)Math.Ceiling((double)gridResolution.Height / areasInStripColumn);

            return
                Enumerable.Range(0, numberOfStrips).
                AsParallel().
                SelectMany(stripIndex => FindEdgeAreasInStrip(cornerCalculator, gridResolution, iterationRange, stripIndex));
        }

        /// <summary>
        /// Finds the edge areas in a strip of points.
        /// </summary>
        /// <param name="cornerCalculator">Calculates corners of edge areas.</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="iterationRange">The iteration range.</param>
        /// <param name="areaStripIndex">Index of the strip of edge areas.</param>
        /// <returns>The edge areas it found.</returns>
        /// <remarks>
        /// This iterates horizontally over a columns of points in a strip.  It keeps track of two columns of results
        /// (left and right). From the columns, it determines which areas have corners that are a mix of points inside
        /// and outside the set.
        /// 
        /// This gets confusing because points and edge areas do not have the same coordinate space (since an edge area
        /// is, well, an area).
        /// </remarks>
        private static IEnumerable<EdgeArea> FindEdgeAreasInStrip(
            CornerCalculator cornerCalculator,
            Size resolution,
            IntRange iterationRange,
            int areaStripIndex)
        {
            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = VectorKernel.VectorCapacity - 1;

            var areaRowsRemaining = resolution.Height - areaStripIndex * areasInStripColumn;
            var areasInBatch = Math.Min(areaRowsRemaining, areasInStripColumn);

            var reals = new double[VectorKernel.VectorCapacity];
            var imags = new double[VectorKernel.VectorCapacity];
            var leftPointColumnIsInSet = new bool[VectorKernel.VectorCapacity];
            var rightPointColumnIsInSet = new bool[VectorKernel.VectorCapacity];

            void IterateRightColumn(int pointColumnIndex)
            {
                var realValue = cornerCalculator.GetRealValue(pointColumnIndex);

                for (int pointRowIndex = 0; pointRowIndex < VectorKernel.VectorCapacity; pointRowIndex++)
                {
                    reals[pointRowIndex] = realValue;
                    imags[pointRowIndex] = cornerCalculator.GetImagValue(areaStripIndex * areasInStripColumn + pointRowIndex);

                    rightPointColumnIsInSet[pointRowIndex] = MandelbulbChecker.IsInsideBulbs(reals[pointRowIndex], imags[pointRowIndex]);
                }

                if (rightPointColumnIsInSet.Any(definitivelyInSet => !definitivelyInSet))
                {
                    var vReals = new Vector<double>(reals);
                    var vImags = new Vector<double>(imags);

                    // Do a quick check with a low bailout to see if all the points are outside the set
                    const int quickBailout = 500;
                    var vIterations = VectorKernel.IteratePoints(vReals, vImags, quickBailout);

                    void CopyResults(long max)
                    {
                        for (int i = 0; i < VectorKernel.VectorCapacity; i++)
                        {
                            rightPointColumnIsInSet[i] = vIterations[i] == max;
                        }
                    }

                    CopyResults(quickBailout);
                    if (rightPointColumnIsInSet.Any(inSet => inSet))
                    {
                        vIterations = VectorKernel.IteratePoints(vReals, vImags, iterationRange.Max);

                        CopyResults(iterationRange.Max);
                    }
                }
            }

            void SwapColumnArrays()
            {
                var temp = leftPointColumnIsInSet;
                leftPointColumnIsInSet = rightPointColumnIsInSet;
                rightPointColumnIsInSet = temp;
            }

            IterateRightColumn(pointColumnIndex: 0);
            SwapColumnArrays();

            for (int rightColumnIndex = 1; rightColumnIndex < (resolution.Width - 1); rightColumnIndex++)
            {
                IterateRightColumn(rightColumnIndex);

                // Iterate the areas bottom to top
                for (int areaIndex = 0; areaIndex < areasInBatch; areaIndex++)
                {
                    var cornersInSet =
                        (leftPointColumnIsInSet[areaIndex] ? Corners.BottomLeft : Corners.None) |
                        (rightPointColumnIsInSet[areaIndex] ? Corners.BottomRight : Corners.None) |
                        (leftPointColumnIsInSet[areaIndex + 1] ? Corners.TopLeft : Corners.None) |
                        (rightPointColumnIsInSet[areaIndex + 1] ? Corners.TopRight : Corners.None);

                    if (cornersInSet != Corners.None && cornersInSet != Corners.All)
                    {
                        yield return new EdgeArea(
                            new Point(rightColumnIndex - 1, areaStripIndex * areasInStripColumn + areaIndex),
                            cornersInSet);
                    }
                }

                SwapColumnArrays();
            }
        }
    }
}
