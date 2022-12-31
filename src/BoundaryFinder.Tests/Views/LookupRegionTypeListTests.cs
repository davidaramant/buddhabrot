using BoundaryFinder.Gui.Views;
using Buddhabrot.Core.Boundary;

namespace BoundaryFinder.Tests.Views;

public class LookupRegionTypeListTests
{
    [Fact]
    public void ShouldReturnCorrectTypesForMultipleRuns()
    {
        var list = new LookupRegionTypeList();
        list.Add(LookupRegionType.MixedDiff, 2);
        list.Add(LookupRegionType.MixedDiff, 3);

        for (int i = 0; i < 5; i++)
        {
            list.Consume().Should().Be(LookupRegionType.MixedDiff);
        }
    }

    [Fact]
    public void ShouldReturnCorrectTypesForMultipleTypes()
    {
        var list = new LookupRegionTypeList();
        list.Add(LookupRegionType.MixedDiff, 2);
        list.Add(LookupRegionType.EmptyToBorder, 3);

        for (int i = 0; i < 2; i++)
        {
            list.Consume().Should().Be(LookupRegionType.MixedDiff);
        }
        for (int i = 0; i < 3; i++)
        {
            list.Consume().Should().Be(LookupRegionType.EmptyToBorder);
        }
    }
}