using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class AreaIdTests
{
    [Fact]
    public void ShouldHaveCorrectXAndY()
    {
        var id = new AreaId((3 << 16) + 2);
        id.X.Should().Be(2);
        id.Y.Should().Be(3);
    }

    [Fact]
    public void ShouldCreateEquivalentIdFromXAndY()
    {
        var id1 = new AreaId((3 << 16) + 2);
        var id2 = new AreaId(2, 3);
        id1.Should().Be(id2);
    }
}