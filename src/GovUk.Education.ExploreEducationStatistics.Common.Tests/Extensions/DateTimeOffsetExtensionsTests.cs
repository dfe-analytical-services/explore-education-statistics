using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class DateTimeOffsetExtensionsTests
{
    public class GetUkStartOfDayUtcTests : DateTimeOffsetExtensionsTests
    {
        [Theory]
        [MemberData(
            nameof(DateTimeOffsetExtensionsTestsTheoryData.GetUkStartOfDayUtcUTheoryData.UtcZoneData),
            MemberType = typeof(DateTimeOffsetExtensionsTestsTheoryData.GetUkStartOfDayUtcUTheoryData)
        )]
        [MemberData(
            nameof(DateTimeOffsetExtensionsTestsTheoryData.GetUkStartOfDayUtcUTheoryData.UkZoneData),
            MemberType = typeof(DateTimeOffsetExtensionsTestsTheoryData.GetUkStartOfDayUtcUTheoryData)
        )]
        public void GetUkStartOfDayUtc_ReturnsExpectedResult(
            DateTimeOffset dateTimeOffset,
            DateTimeOffset expectedDateTimeOffset,
            string description
        )
        {
            var actual = dateTimeOffset.GetUkStartOfDayUtc();

            Assert.Equal(TimeSpan.Zero, actual.Offset);
            Assert.True(
                expectedDateTimeOffset.Equals(actual),
                $"Expected: {expectedDateTimeOffset:o}\nActual:   {actual:o}\nDescription: {description}"
            );
        }
    }

    public class ToUkDateOnlyTests : DateTimeOffsetExtensionsTests
    {
        [Theory]
        [MemberData(
            nameof(DateTimeOffsetExtensionsTestsTheoryData.ToUkDateOnlyTheoryData.UtcZoneData),
            MemberType = typeof(DateTimeOffsetExtensionsTestsTheoryData.ToUkDateOnlyTheoryData)
        )]
        [MemberData(
            nameof(DateTimeOffsetExtensionsTestsTheoryData.ToUkDateOnlyTheoryData.UkZoneData),
            MemberType = typeof(DateTimeOffsetExtensionsTestsTheoryData.ToUkDateOnlyTheoryData)
        )]
        public void ToUkDateOnly_ReturnsExpectedResult(
            DateTimeOffset dateTimeOffset,
            DateOnly expectedDateOnly,
            string description
        )
        {
            var actual = dateTimeOffset.ToUkDateOnly();

            Assert.True(
                expectedDateOnly.Equals(actual),
                $"Expected: {expectedDateOnly:o}\nActual:   {actual:o}\nDescription: {description}"
            );
        }
    }

    public class TruncateNanosecondsTests : DateTimeOffsetExtensionsTests
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
