using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public sealed record BoundaryDataSet(
    bool IsDiff,
    BoundaryParameters Parameters,
    string Description)
{
    public override string ToString() => Description;

    public static BoundaryDataSet FromBoundary(BoundaryParameters parameters) =>
        new(IsDiff: false, parameters, parameters.Description);

    public static BoundaryDataSet FromDiff(BoundaryParameters left, BoundaryParameters right) =>
        new(
            IsDiff: true,
            Combine(left, right),
            Description: $"Diff - {left.Description} - {right.Description}");

    private static BoundaryParameters Combine(BoundaryParameters one, BoundaryParameters two) => new(
        new AreaDivisions(Math.Max(one.Divisions.VerticalPower, two.Divisions.VerticalPower)),
        Math.Max(one.MaxIterations, two.MaxIterations));

    public static BoundaryDataSet FromDescription(string description)
    {
        if (description.StartsWith("Diff"))
        {
            var parts = description.Split("-");
            return FromDiff(
                BoundaryParameters.FromDescription(parts[1].Trim()),
                BoundaryParameters.FromDescription(parts[2].Trim()));
        }
        else
        {
            return FromBoundary(BoundaryParameters.FromDescription(description));
        }
    }
}