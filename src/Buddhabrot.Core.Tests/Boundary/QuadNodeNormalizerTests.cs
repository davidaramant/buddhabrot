using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadNodeNormalizerTests
{
    [Theory]
    [InlineData(RegionType.Border, RegionType.Unknown, RegionType.Unknown, RegionType.Unknown, RegionType.Border)]
    [InlineData(RegionType.Border, RegionType.Filament, RegionType.Rejected, RegionType.Unknown, RegionType.Border)]
    [InlineData(RegionType.Border, RegionType.Filament, RegionType.Filament, RegionType.Unknown, RegionType.Filament)]
    public void ShouldCondenseRegionTypes(
        RegionType ll,
        RegionType lr,
        RegionType ul,
        RegionType ur,
        RegionType expected)
    {
        QuadNodeNormalizer.CondenseRegionType(ll, lr, ul, ur).Should().Be(expected);
    }

    [Fact]
    public void ShouldCondenseIdenticalLeaves()
    {
        var normalizer = new QuadNodeNormalizer(new List<QuadNode>());
        var node = normalizer.MakeQuad(
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf);

        node.Should().Be(QuadNode.UnknownLeaf);
    }

    [Fact]
    public void ShouldCreateLeafQuad()
    {
        var normalizer = new QuadNodeNormalizer(new List<QuadNode>());
        var node = normalizer.MakeQuad(
            QuadNode.MakeLeaf(RegionType.Border),
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf);

        node.Should().Be(
            QuadNode.MakeLeaf(
                RegionType.Border,
                RegionType.Border,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown));
    }
}