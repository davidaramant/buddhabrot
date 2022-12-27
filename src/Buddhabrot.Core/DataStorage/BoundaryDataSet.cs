using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public sealed record BoundaryDataSet(
    bool IsDiff,
    BoundaryParameters Parameters,
    string Description,
    string UserDescription) : IComparable<BoundaryDataSet>
{
    public static BoundaryDataSet Empty =>
        new(false, new BoundaryParameters(new AreaDivisions(0), 0), "Nothing", "Nothing");
    
    public int CompareTo(BoundaryDataSet? other)
    {
        // Diffs come last
        // Higher vertical powers come first
        // Then the max iterations (higher first)
        
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var diff = IsDiff.CompareTo(other.IsDiff);
        if (diff != 0)
            return diff;

        var power = other.Parameters.Divisions.VerticalPower.CompareTo(Parameters.Divisions.VerticalPower);
        if (power != 0)
            return power;

        return other.Parameters.MaxIterations.CompareTo(Parameters.MaxIterations);
    }
    
    public override string ToString() => UserDescription;

    public static BoundaryDataSet FromBoundary(BoundaryParameters parameters) =>
        new(IsDiff: false, parameters, parameters.Description, parameters.ToString());

    public static BoundaryDataSet FromDiff(BoundaryParameters left, BoundaryParameters right)
    {
        var description = $"Diff - {left.Description} - {right.Description}";
        
        return new(
            IsDiff: true,
            Combine(left, right),
            Description: description,
            UserDescription: description);
    }

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