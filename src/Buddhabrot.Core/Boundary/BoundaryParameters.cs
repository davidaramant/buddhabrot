using System.Globalization;
using System.Text.RegularExpressions;

namespace Buddhabrot.Core.Boundary;

public sealed record BoundaryParameters(
    int VerticalDivisionsPower,
    int MaxIterations)
{
    public int VerticalDivisions => 1 << VerticalDivisionsPower;
    
    public double SideLength => 2.0 / VerticalDivisions;

    public override string ToString() =>
        $"Vertical Divisions: {VerticalDivisions:N0} Max Iterations: {MaxIterations:N0}";

    public string Description => $"v{VerticalDivisionsPower:N0}_i{MaxIterations:N0}";

    public static BoundaryParameters FromDescription(string description)
    {
        var descriptionRegex = new Regex(@"v([\d,]+)_i([\d\,]+)", RegexOptions.Compiled);
        var match = descriptionRegex.Match(description);
        return new BoundaryParameters(
            VerticalDivisionsPower: int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands),
            MaxIterations: int.Parse(match.Groups[2].Value, NumberStyles.AllowThousands));
    }
}