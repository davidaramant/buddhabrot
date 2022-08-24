using System.Globalization;
using System.Text.RegularExpressions;

namespace Buddhabrot.Core.Boundary;

public sealed record BoundaryParameters(
    AreaDivisions Divisions,
    int MaxIterations)
{
    public override string ToString() =>
        $"Vertical Divisions: {Divisions.QuadrantDivisions:N0} Max Iterations: {MaxIterations:N0}";

    public string Description => $"v{Divisions.VerticalPower:N0}_i{MaxIterations:N0}";

    public static BoundaryParameters FromDescription(string description)
    {
        var descriptionRegex = new Regex(@"v([\d,]+)_i([\d\,]+)", RegexOptions.Compiled);
        var match = descriptionRegex.Match(description);
        return new BoundaryParameters(
            Divisions: new AreaDivisions(int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands)),
            MaxIterations: int.Parse(match.Groups[2].Value, NumberStyles.AllowThousands));
    }
}