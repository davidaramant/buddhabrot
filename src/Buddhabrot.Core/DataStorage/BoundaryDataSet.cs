using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public sealed record BoundaryDataSet(
    bool IsDiff,
    BoundaryParameters Parameters,
    string Description,
    string UserDescription) : IComparable<BoundaryDataSet>
{
    private sealed class IsDiffDescriptionRelationalComparer : IComparer<BoundaryDataSet>
    {
        public int Compare(BoundaryDataSet? x, BoundaryDataSet? y)
        {
            // Diffs come last
            // Higher vertical powers come first
            // Then the max iterations (higher first)
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

        
            var diff = x.IsDiff.CompareTo(y.IsDiff);
            if (diff != 0)
                return diff;

            var power = y.Parameters.Divisions.VerticalPower.CompareTo(x.Parameters.Divisions.VerticalPower);
            if (power != 0)
                return power;

            return y.Parameters.MaxIterations.CompareTo(x.Parameters.MaxIterations);
        }
    }

    public static IComparer<BoundaryDataSet> Comparer { get; } = new IsDiffDescriptionRelationalComparer();

    public static readonly BoundaryDataSet Empty =
        new(false, new BoundaryParameters(new AreaDivisions(0), 0), "Nothing", "Nothing");

    public int CompareTo(BoundaryDataSet? other) => Comparer.Compare(this, other);
    
    public override string ToString() => UserDescription;

    public static BoundaryDataSet FromBoundary(BoundaryParameters parameters) =>
        new(IsDiff: false, parameters, parameters.Description, parameters.ToString());

    public static BoundaryDataSet FromDiff(BoundaryParameters left, BoundaryParameters right)
    {
        static string Shorthand(BoundaryParameters bp) => $"({bp.Divisions.VerticalPower}) {bp.MaxIterations:N0}";
        
        return new(
            IsDiff: true,
            Combine(left, right),
            Description: $"Diff - {left.Description} - {right.Description}",
            UserDescription: $"Diff - {Shorthand(left)} | {Shorthand(right)}");
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