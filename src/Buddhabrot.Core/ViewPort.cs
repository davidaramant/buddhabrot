using System.Drawing;
using System.Numerics;

namespace Buddhabrot.Core;

public sealed class ViewPort
{
    public ComplexArea Area { get; }
    public Size Resolution { get; }

    private readonly double _realIncrement;
    private readonly double _imagIncrement;

    public ViewPort(ComplexArea area, Size resolution)
    {
        Area = area;
        Resolution = resolution;

        // The right-most/top-most point is at the edge of the view port
        _realIncrement = area.RealRange.Magnitude / (resolution.Width - 1);
        _imagIncrement = area.ImagRange.Magnitude / (resolution.Height - 1);
    }

    private int FlipY(int y) => Resolution.Height - y - 1;
    private double GetRealValue(int x) => Area.RealRange.InclusiveMin + x * _realIncrement;
    private double GetImagValue(int y) => Area.ImagRange.InclusiveMin + FlipY(y) * _imagIncrement;

    public Complex GetComplex(int x, int y) => new(GetRealValue(x), GetImagValue(y));
    public Complex GetComplex(Point position) => GetComplex(position.X, position.Y);

    public Rectangle GetRectangle(ComplexArea area) =>
        new Rectangle(
            GetPosition(area.TopLeftCorner),
            new Size((int) Math.Ceiling(area.RealRange.Magnitude * _realIncrement),
                (int) Math.Ceiling(area.ImagRange.Magnitude * _imagIncrement)));

    public Point GetPosition(Complex c) => new(
        (int) ((c.Real - Area.RealRange.InclusiveMin) / _realIncrement),
        FlipY((int) ((c.Imaginary - Area.ImagRange.InclusiveMin) / _imagIncrement)));

    public override string ToString() => $"Area: {Area}, Resolution: {Resolution.Width}x{Resolution.Height}";
}