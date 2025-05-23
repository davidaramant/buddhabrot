using System.Numerics;
using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Core;

public sealed record ComplexViewport(ComplexArea LogicalArea, SKSizeI Resolution)
{
	public double PixelWidth { get; } = LogicalArea.RealInterval.Magnitude / (Resolution.Width - 1);
	public double HalfPixelWidth => PixelWidth / 2;

	public static ComplexViewport FromLogicalArea(ComplexArea area, int width)
	{
		var aspectRatio = area.RealInterval.Magnitude / area.ImagInterval.Magnitude;
		var height = (int)(width / aspectRatio);

		return new(area, new SKSizeI(width, height));
	}

	public static ComplexViewport FromRegionId(SKSizeI resolution, AreaDivisions divisions, RegionId regionId)
	{
		var r = (divisions.RegionSideLength * regionId.X) - 2;
		var i = divisions.RegionSideLength * regionId.Y;

		return FromResolution(resolution, new Complex(r, i), divisions.RegionSideLength);
	}

	public static ComplexViewport FromResolution(SKSizeI resolution, Complex bottomLeft, double realMagnitude)
	{
		var aspectRatio = (double)resolution.Width / resolution.Height;
		var imagMagnitude = realMagnitude / aspectRatio;
		var realInterval = Interval.FromMinAndLength(bottomLeft.Real, realMagnitude);
		var imagInterval = Interval.FromMinAndLength(bottomLeft.Imaginary, imagMagnitude);
		return new(new ComplexArea(realInterval, imagInterval), resolution);
	}

	public static ComplexViewport FromResolution(SKSizeI resolution, SKPointI originOffset, double pixelSize)
	{
		var realMagnitude = resolution.Width * pixelSize;
		var imagMagnitude = resolution.Height * pixelSize;

		var realInterval = Interval.FromMinAndLength(-originOffset.X * pixelSize, realMagnitude);
		var imagInterval = Interval.FromMinAndLength((originOffset.Y - resolution.Height) * pixelSize, imagMagnitude);

		return new(new ComplexArea(realInterval, imagInterval), resolution);
	}

	private int FlipY(int y) => Resolution.Height - y - 1;

	private double GetRealValue(int x) => LogicalArea.RealInterval.InclusiveMin + x * PixelWidth;

	private double GetImagValue(int y) => LogicalArea.ImagInterval.InclusiveMin + FlipY(y) * PixelWidth;

	public Complex GetComplex(int x, int y) => new(GetRealValue(x), GetImagValue(y));

	public Complex GetComplex(SKPointI position) => GetComplex(position.X, position.Y);

	public SKRectI GetRectangle(ComplexArea area) =>
		new(
			GetPosition(area.TopLeftCorner).X,
			GetPosition(area.TopLeftCorner).Y,
			(int)Math.Ceiling(area.RealInterval.Magnitude / PixelWidth),
			(int)Math.Ceiling(area.ImagInterval.Magnitude / PixelWidth)
		);

	public SKPointI GetPosition(Complex c) =>
		new(
			(int)((c.Real - LogicalArea.RealInterval.InclusiveMin) / PixelWidth),
			FlipY((int)((c.Imaginary - LogicalArea.ImagInterval.InclusiveMin) / PixelWidth))
		);

	public override string ToString() => $"Area: {LogicalArea}, Resolution: {Resolution.Width}x{Resolution.Height}";
}
