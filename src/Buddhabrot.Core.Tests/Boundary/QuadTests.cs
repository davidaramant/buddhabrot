using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadTests
{
    public sealed record QuadTestCase(Quad Node, RegionType ExpectedType);

    public static IEnumerable<object[]> LeafNodeTestCases()
    {
        yield return new object[] { new QuadTestCase(Quad.EmptyLeaf, RegionType.Empty) };
        yield return new object[] { new QuadTestCase(Quad.BorderLeaf, RegionType.Border) };
        yield return new object[] { new QuadTestCase(Quad.FilamentLeaf, RegionType.Filament) };
    }

    [Theory]
    [MemberData(nameof(LeafNodeTestCases))]
    public void ShouldConstructLeafNodesCorrectly(QuadTestCase testCase)
    {
        testCase.Node.Type.Should().Be(testCase.ExpectedType);
        testCase.Node.HasChildren.Should().BeFalse();
    }

    public static IEnumerable<object[]> ParentNodeTestCases()
    {
        yield return new object[] { new QuadTestCase(new Quad(RegionType.Empty, 123), RegionType.Empty) };
        yield return new object[] { new QuadTestCase(new Quad(RegionType.Border, 123), RegionType.Border) };
        yield return new object[] { new QuadTestCase(new Quad(RegionType.Filament, 123), RegionType.Filament) };
    }

    [Theory]
    [MemberData(nameof(ParentNodeTestCases))]
    public void ShouldConstructParentNodesCorrectly(QuadTestCase testCase)
    {
        var node = testCase.Node;

        node.Type.Should().Be(testCase.ExpectedType);
        node.HasChildren.Should().BeTrue();
        node.ChildIndex.Should().Be(123);
    }

    [Fact]
    public void ShouldHaveExpectedEquality()
    {
        Quad.EmptyLeaf.Should().Be(Quad.EmptyLeaf);
        Quad.BorderLeaf.Should().Be(Quad.BorderLeaf);
        Quad.FilamentLeaf.Should().Be(Quad.FilamentLeaf);

        Quad.EmptyLeaf.Should().NotBe(Quad.BorderLeaf);
        Quad.EmptyLeaf.Should().NotBe(Quad.FilamentLeaf);
        Quad.BorderLeaf.Should().NotBe(Quad.FilamentLeaf);

        new Quad(RegionType.Empty, 0).Should().NotBe(Quad.EmptyLeaf);
    }
}