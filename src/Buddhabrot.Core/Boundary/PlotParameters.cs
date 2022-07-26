namespace Buddhabrot.Core.Boundary;

public record PlotParameters(
    int VerticalDivisions,
    IterationRange IterationRange)
{
    public double SideLength => 2.0 / VerticalDivisions;
    
    public ComplexArea GetAreaOfId(AreaId id)
    {
        var realStart = -2 + id.X * SideLength;
        var imagStart = id.Y * SideLength;
        
        return new ComplexArea(
            RealRange: new DoubleRange(InclusiveMin: realStart, ExclusiveMax: realStart + SideLength),
            ImagRange: new DoubleRange(InclusiveMin: imagStart, ExclusiveMax: imagStart + SideLength));
    }
}