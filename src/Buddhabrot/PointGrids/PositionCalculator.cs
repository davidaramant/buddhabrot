using System.Drawing;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.PointGrids
{
    public sealed class PositionCalculator
    {
        private readonly ComplexArea _viewPort;
        private readonly double _realIncrement;
        private readonly double _imagIncrement;

        public PositionCalculator(Size pointResolution, ComplexArea viewPort)
        {
            _viewPort = viewPort;

            // The right-most/top-most point is at the edge of the view port
            _realIncrement = viewPort.RealRange.Magnitude / (pointResolution.Width - 1);
            _imagIncrement = viewPort.ImagRange.Magnitude / (pointResolution.Height - 1);
        }

        public double GetRealValue(int x) => _viewPort.RealRange.InclusiveMin + x * _realIncrement;
        public double GetImagValue(int y) => _viewPort.ImagRange.InclusiveMin + y * _imagIncrement;

        public Complex GetPoint(int x, int y) => new Complex(GetRealValue(x), GetImagValue(y));
        public Complex GetPoint(Point position) => GetPoint(position.X, position.Y);
    }
}
