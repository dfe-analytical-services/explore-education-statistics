using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

// ReSharper disable once ClassNeverInstantiated.Global
public abstract class DateOnlyExtensionsTests
{
    public class GetUkStartOfDayUtc : DateOnlyExtensionsTests
    {
        [Theory]
        [InlineData("2025-01-01", "2025-01-01T00:00:00 +00:00")] // Day within GMT
        [InlineData("2025-06-01", "2025-05-31T23:00:00 +00:00")] // Day within BST
        [InlineData("2025-03-30", "2025-03-30T00:00:00 +00:00")] // BST starts on this day (start of day is within GMT)
        [InlineData("2025-03-31", "2025-03-30T23:00:00 +00:00")] // Day after BST starts (start of day is within BST)
        [InlineData("2025-10-26", "2025-10-25T23:00:00 +00:00")] // BST ends on this day (start of day is within BST)
        [InlineData("2025-10-27", "2025-10-27T00:00:00 +00:00")] // Day after BST ends (start of day is within GMT)
        public void GetUkStartOfDayUtc_ReturnsExpectedResult(string dateOnlyString, string expectedDateTimeOffsetString)
        {
            var dateOnly = DateOnly.Parse(dateOnlyString);
            var expectedDateTimeOffset = DateTimeOffset.Parse(expectedDateTimeOffsetString);

            var actual = dateOnly.GetUkStartOfDayUtc();

            Assert.Equal(TimeSpan.Zero, actual.Offset);
            Assert.Equal(expectedDateTimeOffset, actual);
        }
    }
}
