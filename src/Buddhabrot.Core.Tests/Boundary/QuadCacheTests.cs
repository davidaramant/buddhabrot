using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadCacheTests
{
    public static IEnumerable<object[]> LeafNodes()
    {
        yield return new object[] { Quad.UnknownLeaf };
        yield return new object[] { Quad.BorderLeaf };
        yield return new object[] { Quad.FilamentLeaf };
    }
    
    [Theory]
    [MemberData(nameof(LeafNodes))]
    public void ShouldCombineLeaves(Quad leaf)
    {
        var nodes = new List<Quad>();
        var cache = new QuadCache(nodes);

        var result = cache.MakeQuad(leaf, leaf, leaf, leaf);
        result.Should().Be(leaf);
        nodes.Should().BeEmpty();
    }

    [Fact]
    public void ShouldNotCombineDissimilarNodes()
    {
        var nodes = new List<Quad>();
        var cache = new QuadCache(nodes);

        var result = cache.MakeQuad(
            Quad.UnknownLeaf, 
            Quad.UnknownLeaf, Quad.UnknownLeaf, Quad.BorderLeaf);
        result.Should().NotBe(Quad.UnknownLeaf);
        result.Should().NotBe(Quad.BorderLeaf);
        result.Should().Be(new Quad(RegionType.Border, 0));
        nodes.Should().HaveCount(4);
    }
    
    
    [Fact]
    public void ShouldCacheEquivalentNodes()
    {
        var nodes = new List<Quad>();
        var cache = new QuadCache(nodes);

        var result1 = cache.MakeQuad(
            Quad.UnknownLeaf, 
            Quad.UnknownLeaf, Quad.UnknownLeaf, Quad.BorderLeaf);
        var result2 = cache.MakeQuad(
            Quad.UnknownLeaf, 
            Quad.UnknownLeaf, Quad.UnknownLeaf, Quad.BorderLeaf);

        result1.Should().Be(result2);
        nodes.Should().HaveCount(4);
        cache.NumCachedValuesUsed.Should().Be(1);
    }
}