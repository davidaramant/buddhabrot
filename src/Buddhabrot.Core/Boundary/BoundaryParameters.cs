using System.Globalization;
using System.Text.RegularExpressions;
using Humanizer;

namespace Buddhabrot.Core.Boundary;

public sealed record BoundaryParameters(
    AreaDivisions Divisions,
    int MaxIterations)
{
    public override string ToString() =>
        $"({Divisions.VerticalPower}) Vertical Divisions: {Divisions.QuadrantDivisions:N0}, Max Iterations: {MaxIterations:N0}";

    public string Description => $"v{Divisions.VerticalPower:N0}_i{MaxIterations.ToMetric()}";

    public static BoundaryParameters FromDescription(string description)
    {
        var descriptionRegex = new Regex(@"v([\d,]+)_i(.+)", RegexOptions.Compiled);
        var match = descriptionRegex.Match(description);
        return new BoundaryParameters(
            Divisions: new AreaDivisions(int.Parse(match.Groups[1].Value, NumberStyles.AllowThousands)),
            MaxIterations: (int)Math.Ceiling(match.Groups[2].Value.FromMetric()));
    }
}