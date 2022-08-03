using System.Numerics;

namespace Buddhabrot.Core.Boundary;

public record PlotParameters(
    int VerticalDivisions,
    IterationRange IterationRange)
{
    public double SideLength => 2.0 / VerticalDivisions;

    public ComplexArea GetAreaOfId(RegionId id)
    {
        var realStart = -2 + id.X * SideLength;
        var imagStart = (id.Y + 1) * SideLength;

        return ComplexArea.SquareFromLowerLeft(new Complex(realStart, imagStart), SideLength);
    }
}