using System.Drawing;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.PointGrids
{
    public sealed class PositionCalculator
    {
        private readonly ComplexArea _viewPort;
        public double RealIncrement { get; }
        public double ImagIncrement { get; }

        public PositionCalculator(Size pointResolution, ComplexArea viewPort)
        {
            _viewPort = viewPort;

            // The right-most/top-most point is at the edge of the view port
            RealIncrement = viewPort.RealRange.Magnitude / (pointResolution.Width - 1);
            ImagIncrement = viewPort.ImagRange.Magnitude / (pointResolution.Height - 1);
        }

        public double GetRealValue(int x) => _viewPort.RealRange.InclusiveMin + x * RealIncrement;
        public double GetImagValue(int y) => _viewPort.ImagRange.InclusiveMin + y * ImagIncrement;

        public Complex GetPoint(int x, int y) => new Complex(GetRealValue(x), GetImagValue(y));
        public Complex GetPoint(Point position) => GetPoint(position.X, position.Y);

        public Point GetPosition(Complex c) => new Point(
            (int)((c.Real - _viewPort.RealRange.InclusiveMin) / RealIncrement),
            (int)((c.Imaginary - _viewPort.ImagRange.InclusiveMin) / ImagIncrement));
    }
}
