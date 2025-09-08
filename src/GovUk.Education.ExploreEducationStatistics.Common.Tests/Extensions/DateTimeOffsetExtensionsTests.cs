using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public class DateTimeOffsetExtensionsTests
{
    public class TruncateNanosecondsTests
    {
        private static readonly DateTimeOffset DateWithoutNanoseconds = new(
            year: 2024,
            month: 02,
            day: 03,
            hour: 4,
            minute: 5,
            second: 6,
            millisecond: 7,
            microsecond: 8,
            TimeSpan.Zero
        );

        private static readonly DateTimeOffset DateWithNanoseconds = DateWithoutNanoseconds.AddTicks(1);

        [Fact]
        public void EqualWithNanosecondDifference()
        {
            Assert.Equal(DateWithNanoseconds.TruncateNanoseconds(), DateWithoutNanoseconds);
        }

        [Fact]
        public void NotEqualWithMicrosecondDifference()
        {
            Assert.NotEqual(DateWithNanoseconds.AddMicroseconds(1).TruncateNanoseconds(), DateWithoutNanoseconds);
        }

        [Fact]
        public void NullDate()
        {
            Assert.Throws<ArgumentException>(() => ((DateTimeOffset?)null).TruncateNanoseconds());
        }
    }
}
