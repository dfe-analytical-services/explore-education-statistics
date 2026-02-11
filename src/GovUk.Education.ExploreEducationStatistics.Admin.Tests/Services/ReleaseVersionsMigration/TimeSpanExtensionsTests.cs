using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ReleaseVersionsMigration;

public abstract class TimeSpanExtensionsTests
{
    public class PrettyPrintTests : TimeSpanExtensionsTests
    {
        [Fact]
        public void WhenTimeSpanIsZero_ReturnsPrettyPrintWithZeroSeconds()
        {
            Assert.Equal("0 seconds", TimeSpan.Zero.PrettyPrint());
        }

        [Fact]
        public void WhenTimeSpanIsLessThanOneSecond_ReturnsPrettyPrintWithZeroSeconds()
        {
            Assert.Equal("0 seconds", TimeSpan.FromMilliseconds(100).PrettyPrint());
        }

        [Fact]
        public void WhenTimeSpanIsNegativeAndLessThanOneSecond_ReturnsPrettyPrintWithNegativePrefixAndZeroSeconds()
        {
            Assert.Equal("-0 seconds", TimeSpan.FromMilliseconds(-100).PrettyPrint());
        }

        [Theory]
        [InlineData(1, 0, 0, 0, "1 day")]
        [InlineData(2, 0, 0, 0, "2 days")]
        [InlineData(0, 1, 0, 0, "1 hour")]
        [InlineData(0, 2, 0, 0, "2 hours")]
        [InlineData(0, 0, 1, 0, "1 minute")]
        [InlineData(0, 0, 2, 0, "2 minutes")]
        [InlineData(0, 0, 0, 1, "1 second")]
        [InlineData(0, 0, 0, 2, "2 seconds")]
        [InlineData(1, 0, 1, 0, "1 day, 1 minute")]
        [InlineData(1, 0, 0, 1, "1 day, 1 second")]
        [InlineData(0, 1, 0, 1, "1 hour, 1 second")]
        [InlineData(0, 0, 1, 1, "1 minute, 1 second")]
        [InlineData(1, 1, 1, 0, "1 day, 1 hour, 1 minute")]
        [InlineData(1, 1, 0, 1, "1 day, 1 hour, 1 second")]
        [InlineData(1, 0, 1, 1, "1 day, 1 minute, 1 second")]
        [InlineData(1, 2, 3, 4, "1 day, 2 hours, 3 minutes, 4 seconds")]
        public void WhenTimeSpanHasMultipleComponents_ReturnsExpectedPrettyPrint(
            int days,
            int hours,
            int minutes,
            int seconds,
            string expected
        )
        {
            var timeSpan = new TimeSpan(days, hours, minutes, seconds);
            Assert.Equal(expected, timeSpan.PrettyPrint());
        }

        [Fact]
        public void WhenTimeSpanIsNegative_ReturnsPrettyPrintWithNegativePrefix()
        {
            var timeSpan = new TimeSpan(1, 2, 3, 4).Negate();
            Assert.Equal("-1 day, 2 hours, 3 minutes, 4 seconds", timeSpan.PrettyPrint());
        }
    }
}
