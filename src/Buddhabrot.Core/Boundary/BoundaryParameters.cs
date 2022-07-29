using System.Globalization;
using System.Text.RegularExpressions;

namespace Buddhabrot.Core.Boundary;

public record BoundaryParameters(
    int VerticalDivisions,
    int MaxIterations)
{
    public double SideLength => 2.0 / VerticalDivisions;

    public override string ToString() =>
        $"Vertical Divisions: {VerticalDivisions:N0} Max Iterations: {MaxIterations:N0}";

    public string Description => $"v{VerticalDivisions:N0}_i{MaxIterations:N0}";

    public static BoundaryParameters FromFilePrefix(string filePrefix)
    {
        var nameRegex = new Regex(@"v([\d,]+)_i([\d\,]+)", RegexOptions.Compiled);
        var match = nameRegex.Match(filePrefix);
        return new BoundaryParameters(
            VerticalDivisions: int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands),
            MaxIterations: int.Parse(match.Groups[2].Value, NumberStyles.AllowThousands));
    }
}