using System;
using System.Collections.Generic;
using System.Linq;

namespace Buddhabrot.PointGrids
{
    public sealed class PointRow : IEnumerable<bool>
    {
        private readonly List<(bool inSet, int start, int length)> _segments = 
            new List<(bool inSet, int start, int length)>();

        public int Y { get; }
        public int Width { get; }

        public PointRow(int width, int y, IEnumerable<(bool inSet, int length)> segments)
        {
            Width = width;
            Y = y;

            int position = 0;
            foreach (var segment in segments)
            {
                _segments.Add((segment.inSet,position,segment.length));
                position += segment.length;
            }

            if (position != Width)
            {
                throw new ArgumentException("The segments do not add up to a full row.");
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
