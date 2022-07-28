namespace Buddhabrot.Core.Boundary;

public record BoundaryParameters(
    int VerticalDivisions,
    int MaxIterations)
{
    public double SideLength => 2.0 / VerticalDivisions;

    public override string ToString() =>
        $"Vertical Divisions: {VerticalDivisions:N0}, Max Iterations: {MaxIterations:N0}";
}