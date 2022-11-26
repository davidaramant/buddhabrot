using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class VisitedRegionsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(0, 2)]
    public void HasVisitedShouldReturnFalseForEmptyTree(int x, int y)
    {
        var tree = new VisitedRegions();

        tree.HasVisited(new RegionId(x, y)).Should().BeFalse();
    }

    [Fact]
    public void ShouldMarkRegionAsVisited()
    {
        var tree = new VisitedRegions();
        
        tree.MarkVisited(new RegionId(0,0), RegionType.Border);

        tree.HasVisited(new RegionId(0, 0)).Should().BeTrue();
        tree.HasVisited(new RegionId(1, 0)).Should().BeFalse();
        tree.HasVisited(new RegionId(0, 1)).Should().BeFalse();
        tree.HasVisited(new RegionId(1, 1)).Should().BeFalse();
    }

    [Fact]
    public void ShouldExpandTree()
    {
        var tree = new VisitedRegions();

        tree.Height.Should().Be(3);
        
        tree.MarkVisited(new RegionId(4,0), RegionType.Border);

        tree.Height.Should().Be(4);

        tree.HasVisited(new RegionId(4, 0)).Should().BeTrue();
    }
}