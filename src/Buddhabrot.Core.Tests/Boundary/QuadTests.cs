using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadTests
{
    public sealed record QuadTestCase(Quad Node, RegionType ExpectedType);

    public static IEnumerable<object[]> LeafNodeTestCases()
    {
        yield return new object[] { new QuadTestCase(Quad.UnknownLeaf, RegionType.Unknown) };
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
        yield return new object[] { new QuadTestCase(new Quad(RegionType.Unknown, 123), RegionType.Unknown) };
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
        Quad.UnknownLeaf.Should().Be(Quad.UnknownLeaf);
        Quad.BorderLeaf.Should().Be(Quad.BorderLeaf);
        Quad.FilamentLeaf.Should().Be(Quad.FilamentLeaf);

        Quad.UnknownLeaf.Should().NotBe(Quad.BorderLeaf);
        Quad.UnknownLeaf.Should().NotBe(Quad.FilamentLeaf);
        Quad.BorderLeaf.Should().NotBe(Quad.FilamentLeaf);

        new Quad(RegionType.Unknown, 0).Should().NotBe(Quad.UnknownLeaf);
    }
}