using Buddhabrot.Core.Boundary;
using Humanizer;

namespace Buddhabrot.Core.DataStorage;

public sealed record BoundaryDataSet(
    bool IsDiff,
    BoundaryParameters Parameters,
    string Description)
{
    public override string ToString() => Description;

    public static BoundaryDataSet FromBoundary(BoundaryParameters parameters) =>
        new(IsDiff: false, parameters, parameters.Description);

    // TODO: Need easy way to condense iterations
    public static BoundaryDataSet FromDiff(BoundaryParameters left, BoundaryParameters right) =>
        new(
            IsDiff: true, 
            Combine(left, right),
            Description: $"Diff - v{left.Divisions.VerticalPower}i{left.MaxIterations.ToMetric()} - v{right.Divisions.VerticalPower}i{right.MaxIterations.ToMetric()}");

    private static BoundaryParameters Combine(BoundaryParameters one, BoundaryParameters two) => new(
        new AreaDivisions(Math.Max(one.Divisions.VerticalPower, two.Divisions.VerticalPower)),
        Math.Max(one.MaxIterations, two.MaxIterations));
}