using System;
using System.Collections.Generic;
using System.Linq;
using Buddhabrot.Core;
using log4net;
using System.Drawing;
using System.Numerics;
using Buddhabrot.Kernels;

namespace Buddhabrot.Edges
{
    static class EdgeLocator
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeLocator));

        public static void FindEdges(
            string outputFilePath,
            ComplexArea viewPort,
            Size gridResolution,
            IterationRange iterationRange)
        {
            // Assumptions for this class
            if (Vector<float>.Count != 8 || Vector<int>.Count != 8)
            {
                throw new ArgumentException($"This assumes a vector capacity of 8 32-bit values, but instead it's {Vector<float>.Count}");
            }
            if (Math.Abs(viewPort.ImagRange.ExclusiveMax + viewPort.ImagRange.InclusiveMin) >= float.Epsilon)
            {
                throw new ArgumentException($"The imaginary range of the viewport must be centered around the real axis.");
            }
            if (gridResolution.Height % 16 != 0)
            {
                throw new ArgumentException($"The vertical resolution of the grid must be divisible by 16.");
            }

            // Divide the view port / resolution in half since the set is symmetrical across the real axis

            var targetViewPort = new ComplexArea(
                realRange: viewPort.RealRange,
                imagRange: new Range(0, viewPort.ImagRange.ExclusiveMax));

            var targetResolution = new Size(width: gridResolution.Width, height: gridResolution.Height / 2);

            Log.Info($"Looking for {targetResolution.Width}x{targetResolution.Height} areas in {targetViewPort}");
            Log.Info($"Saving edges to: {outputFilePath}");
            Log.Info($"Iteration count: {iterationRange}");

            EdgeAreas.Write(
                outputFilePath, 
                gridResolution,
                viewPort,
                GetEdgeAreas(targetViewPort, targetResolution, iterationRange));
        }

        private static IEnumerable<ComplexArea> GetEdgeAreas(ComplexArea viewPort, Size resolution, IterationRange iterationRange)
        {
            var vectorSize = Vector<float>.Count;

            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = vectorSize - 1;

            var numberOfStrips = (int)Math.Ceiling((double)resolution.Height / areasInStripColumn);

            return
                Enumerable.Range(0, numberOfStrips).
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
        private static IEnumerable<ComplexArea> FindEdgeAreasInStrip(
            ComplexArea viewPort,
            Size resolution,
            IterationRange iterationRange,
            int stripIndex)
        {
            var vectorSize = Vector<float>.Count;

            // An area has both a bottom and top row, so we can only fit n-1 areas into a strip.
            var areasInStripColumn = vectorSize - 1;

            var areaRowsRemaining = resolution.Height - stripIndex * areasInStripColumn;
            var areasInBatch = Math.Min(areaRowsRemaining, areasInStripColumn);

            float realIncrement = viewPort.RealRange.Magnitude / (resolution.Width - 1);
            float imagIncrement = viewPort.ImagRange.Magnitude / (resolution.Height - 1);

            var reals = new float[vectorSize];
            var imags = new float[vectorSize];
            var leftColumnIsInSet = new bool[vectorSize];
            var rightColumnIsInSet = new bool[vectorSize];

            float GetRealValue(int columnIndex) => viewPort.RealRange.InclusiveMin + columnIndex * realIncrement;
            float GetImagValue(int rowIndex) => viewPort.ImagRange.InclusiveMin + (stripIndex * areasInStripColumn + rowIndex) * imagIncrement;

            void IterateRightColumn(int columnIndex)
            {
                var realValue = GetRealValue(columnIndex);

                for (int rowIndex = 0; rowIndex < vectorSize; rowIndex++)
                {
                    reals[rowIndex] = realValue;
                    imags[rowIndex] = GetImagValue(rowIndex);
                }

                var vReals = new Vector<float>(reals);
                var vImags = new Vector<float>(imags);

                var vIterations = VectorKernel.IteratePoints(vReals, vImags, iterationRange.ExclusiveMax);

                for (int i = 0; i < vectorSize; i++)
                {
                    rightColumnIsInSet[i] = vIterations[i] == iterationRange.ExclusiveMax;
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

                bool IsEdgeArea(bool leftBottomCorner, bool rightBottomCorner, bool leftTopCorner, bool rightTopCorner)
                {
                    return
                        leftBottomCorner != rightBottomCorner ||
                        rightBottomCorner != leftTopCorner ||
                        leftTopCorner != rightTopCorner;
                }

                // Iterate the areas bottom to top
                for (int areaIndex = 0; areaIndex < areasInBatch; areaIndex++)
                {
                    if (IsEdgeArea(
                        leftBottomCorner: leftColumnIsInSet[areaIndex],
                        rightBottomCorner: rightColumnIsInSet[areaIndex],
                        leftTopCorner: leftColumnIsInSet[areaIndex + 1],
                        rightTopCorner: rightColumnIsInSet[areaIndex + 1]))
                    {
                        yield return new ComplexArea(
                            realRange: new Range(GetRealValue(rightColumnIndex - 1), GetRealValue(rightColumnIndex)),
                            imagRange: new Range(GetImagValue(areaIndex), GetImagValue(areaIndex + 1)));
                    }
                }

                SwapColumnArrays();
            }
        }
    }
}
