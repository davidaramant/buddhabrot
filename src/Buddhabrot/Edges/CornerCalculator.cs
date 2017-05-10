using System;
using System.Drawing;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    public sealed class CornerCalculator
    {
        private readonly ComplexArea _viewPort;
        private readonly double _realIncrement;
        private readonly double _imagIncrement;

        public CornerCalculator(Size gridSize, ComplexArea viewPort)
        {
            _viewPort = viewPort;

            _realIncrement = viewPort.RealRange.Magnitude / gridSize.Width;
            _imagIncrement = viewPort.ImagRange.Magnitude / gridSize.Height;
        }

        private double GetRealValue(int x) => _viewPort.RealRange.InclusiveMin + x * _realIncrement;
        private double GetImagValue(int y) => _viewPort.ImagRange.InclusiveMin + y * _imagIncrement;

        public Complex GetCorner(Point edgeLocation, Corners corner)
        {
            switch (corner)
            {
                case Corners.BottomLeft:
                    return new Complex(
                        GetRealValue(edgeLocation.X),
                        GetImagValue(edgeLocation.Y));
                case Corners.BottomRight:
                    return new Complex(
                        GetRealValue(edgeLocation.X + 1),
                        GetImagValue(edgeLocation.Y));
                case Corners.TopLeft:
                    return new Complex(
                        GetRealValue(edgeLocation.X),
                        GetImagValue(edgeLocation.Y + 1));
                case Corners.TopRight:
                    return new Complex(
                        GetRealValue(edgeLocation.X + 1),
                        GetImagValue(edgeLocation.Y + 1));
                default:
                    throw new ArgumentException();
            }
        }
    }
}
