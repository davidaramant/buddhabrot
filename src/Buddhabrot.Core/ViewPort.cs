﻿using System.Drawing;
using System.Numerics;

namespace Buddhabrot.Core;

public sealed class ViewPort
{
    public ComplexArea LogicalArea { get; }
    public Size Resolution { get; }

    public double PixelWidth { get; }
    public double HalfPixelWidth => PixelWidth / 2;

    private ViewPort(ComplexArea logicalArea, Size resolution)
    {
        LogicalArea = logicalArea;
        Resolution = resolution;
        
        PixelWidth = logicalArea.RealInterval.Magnitude / (resolution.Width - 1);
    }

    public static ViewPort FromLogicalArea(ComplexArea area, int width)
    {
        var aspectRatio = area.RealInterval.Magnitude / area.ImagInterval.Magnitude;
        var height = (int)(width / aspectRatio);

        return new(area, new Size(width, height));
    }

    public static ViewPort FromResolution(Size resolution, Complex bottomLeft, double realMagnitude)
    {
        var aspectRatio = (double)resolution.Width / resolution.Height;
        var imagMagnitude = realMagnitude / aspectRatio;
        var realInterval = Interval.FromMinAndLength(bottomLeft.Real, realMagnitude);
        var imagInterval = Interval.FromMinAndLength(bottomLeft.Imaginary, imagMagnitude);
        return new(new ComplexArea(realInterval, imagInterval), resolution);
    }

    private int FlipY(int y) => Resolution.Height - y - 1;
    private double GetRealValue(int x) => LogicalArea.RealInterval.InclusiveMin + x * PixelWidth;
    private double GetImagValue(int y) => LogicalArea.ImagInterval.InclusiveMin + FlipY(y) * PixelWidth;

    public Complex GetComplex(int x, int y) => new(GetRealValue(x), GetImagValue(y));
    public Complex GetComplex(Point position) => GetComplex(position.X, position.Y);

    public Rectangle GetRectangle(ComplexArea area) =>
        new (
            GetPosition(area.TopLeftCorner),
            new Size((int) Math.Ceiling(area.RealInterval.Magnitude / PixelWidth),
                (int) Math.Ceiling(area.ImagInterval.Magnitude / PixelWidth)));

    public Point GetPosition(Complex c) => new(
        (int) ((c.Real - LogicalArea.RealInterval.InclusiveMin) / PixelWidth),
        FlipY((int) ((c.Imaginary - LogicalArea.ImagInterval.InclusiveMin) / PixelWidth)));

    public override string ToString() => $"Area: {LogicalArea}, Resolution: {Resolution.Width}x{Resolution.Height}";
}