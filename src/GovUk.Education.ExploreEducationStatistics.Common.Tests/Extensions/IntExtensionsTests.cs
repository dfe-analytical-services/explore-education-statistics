using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public class IntExtensionsTests
    {
        public class ToEnumerable
        {
            [Fact]
            public void CountEqualToPositiveIntValue()
            {
                Assert.Equal(5, 5.ToEnumerable().Count());
            }

            [Fact]
            public void CountEqualToNegativeIntValue()
            {
                Assert.Equal(-5, -5.ToEnumerable().Count());
            }

            [Fact]
            public void ItemsMatchSeriesUpToPositiveIntValue()
            {
                var ints = 5.ToEnumerable().ToList();

                Assert.Equal(1, ints[0]);
                Assert.Equal(2, ints[1]);
                Assert.Equal(3, ints[2]);
                Assert.Equal(4, ints[3]);
                Assert.Equal(5, ints[4]);
            }

            [Fact]
            public void ItemsMatchSeriesUpToNegativeIntValue()
            {
                var ints = (-5).ToEnumerable().ToList();

                Assert.Equal(-1, ints[0]);
                Assert.Equal(-2, ints[1]);
                Assert.Equal(-3, ints[2]);
                Assert.Equal(-4, ints[3]);
                Assert.Equal(-5, ints[4]);
            }

            [Fact]
            public void ZeroHasNoItems()
            {
                Assert.Empty(0.ToEnumerable());
            }
        }
    }
}