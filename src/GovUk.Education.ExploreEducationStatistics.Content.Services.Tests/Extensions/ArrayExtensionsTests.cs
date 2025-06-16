using System.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void Shuffle_should_not_reorder_items_in_source_array()
    {
        var source = Enumerable.Range(1,10).ToArray();
        var expected = source.ToArray();
        var _ = source.Shuffle();
        Assert.Equal(expected, source);
    }
    
    [Fact]
    public void Shuffle_should_retain_all_items_in_source_array()
    {
        var source = Enumerable.Range(1,100).ToArray();
        var actual = source.Shuffle();
        Assert.Equal(source, actual.OrderBy(i => i));
    }
    
    [Fact]
    public void Shuffle_should_reorder_items_in_array()
    {
        var source = Enumerable.Range(1,10).ToArray();
        var actual = source.Shuffle();
        Assert.NotEqual(source, actual);
    }
}
