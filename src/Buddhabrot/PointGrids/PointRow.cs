using System;
using System.Collections.Generic;
using System.Linq;

namespace Buddhabrot.PointGrids
{
    /// <summary>
    /// Stores whether the points in the row are inside the Mandelbrot set.
    /// </summary>
    public sealed class PointRow : IEnumerable<bool>
    {
        private readonly List<(bool inSet, int start, int length)> _segments =
            new List<(bool inSet, int start, int length)>();

        public int Y { get; }
        public int Width { get; }

        public bool this[int pointIndex]
        {
            get
            {
                var indexOfSegmentPastPointIndex = _segments.FindIndex(s => s.start > pointIndex);

                var segment =
                    indexOfSegmentPastPointIndex == -1
                    ? _segments.Last()
                    : _segments[indexOfSegmentPastPointIndex - 1];

                return segment.inSet;
            }
        }

        public PointRow(int width, int y, IEnumerable<(bool inSet, int length)> segments)
        {
            Width = width;
            Y = y;

            int position = 0;
            foreach (var segment in segments)
            {
                _segments.Add((segment.inSet, position, segment.length));
                position += segment.length;
            }

            if (position != Width)
            {
                throw new ArgumentException($"The segments should have added up to {width} but reached {position}.");
            }
        }

        public IEnumerable<int> GetXPositionsOfSetEdges()
        {
            foreach (var segment in _segments.Where(s => s.inSet))
            {
                yield return segment.start;
                if (segment.length > 1)
                {
                    yield return segment.start + segment.length - 1;
                }
            }
        }

        public IEnumerator<bool> GetEnumerator()
        {
            return _segments.SelectMany(segment => Enumerable.Repeat(segment.inSet, segment.length)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
