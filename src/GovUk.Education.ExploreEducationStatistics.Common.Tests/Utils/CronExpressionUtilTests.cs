#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class CronExpressionUtilTests
{
    public class CronExpressionHasSecondPrecisionTests : CronExpressionUtilTests
    {
        [Fact]
        public void CronExpressionHasSecondPrecision_ReturnsTrue_WhenSecondsAreIncluded()
        {
            Assert.True(CronExpressionUtil.CronExpressionHasSecondPrecision("0 30 9 * * *"));
        }

        [Fact]
        public void CronExpressionHasSecondPrecision_ReturnsFalse_WhenSecondsAreNotIncluded()
        {
            Assert.False(CronExpressionUtil.CronExpressionHasSecondPrecision("30 9 * * *"));
        }
    }

    public class GetNextOccurrenceTests : CronExpressionUtilTests
    {
        [Theory]
        [InlineData("2025-01-01T09:29:59 +00:00", "2025-01-01T09:30:00 +00:00")]
        [InlineData("2025-01-01T09:30:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-01T17:00:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-01T23:59:59 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-02T00:00:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUtcTimezone(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, TimeZoneInfo.Utc);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-01-01T09:29:59 +00:00", "2025-01-01T09:30:00 +00:00")]
        [InlineData("2025-01-01T09:30:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-01T17:00:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-01T23:59:59 +00:00", "2025-01-02T09:30:00 +00:00")]
        [InlineData("2025-01-02T00:00:00 +00:00", "2025-01-02T09:30:00 +00:00")]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-04-01T08:29:59 +00:00", "2025-04-01T09:30:00 +01:00")]
        [InlineData("2025-04-01T08:30:00 +00:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-01T16:00:00 +00:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-01T22:59:59 +00:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-01T23:00:00 +00:00", "2025-04-02T09:30:00 +01:00")]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DuringDaylightSavingTime_FromIsUtc(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }
        
        [Theory]
        [InlineData("2025-04-01T09:29:59 +01:00", "2025-04-01T09:30:00 +01:00")]
        [InlineData("2025-04-01T09:30:00 +01:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-01T17:00:00 +01:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-01T23:59:59 +01:00", "2025-04-02T09:30:00 +01:00")]
        [InlineData("2025-04-02T00:00:00 +01:00", "2025-04-02T09:30:00 +01:00")]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DuringDaylightSavingTime_FromIsLocalTime(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-03-30T00:00:00 +00:00", "2025-03-30T09:30:00 +01:00")] // One hour before BST starts
        [InlineData("2025-03-30T00:59:59 +00:00", "2025-03-30T09:30:00 +01:00")] // BST about to start
        [InlineData("2025-03-30T01:00:00 +00:00", "2025-03-30T09:30:00 +01:00")] // BST started
        [InlineData("2025-03-30T01:00:01 +00:00", "2025-03-30T09:30:00 +01:00")] // One second after
        [InlineData("2025-03-30T02:00:00 +00:00", "2025-03-30T09:30:00 +01:00")] // One hour after
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DaylightSavingTimeStarting_FromIsUtc(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-03-30T00:00:00 +00:00", "2025-03-30T09:30:00 +01:00")] // One hour before BST starts
        [InlineData("2025-03-30T00:59:59 +00:00", "2025-03-30T09:30:00 +01:00")] // BST about to start
        [InlineData("2025-03-30T02:00:00 +01:00", "2025-03-30T09:30:00 +01:00")] // BST started
        [InlineData("2025-03-30T02:00:01 +01:00", "2025-03-30T09:30:00 +01:00")] // One second after
        [InlineData("2025-03-30T03:00:00 +01:00", "2025-03-30T09:30:00 +01:00")] // One hour after
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DaylightSavingTimeStarting_FromIsLocalTime(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-10-26T00:00:00 +00:00", "2025-10-26T09:30:00 +00:00")] // One hour before BST ends
        [InlineData("2025-10-26T00:59:59 +00:00", "2025-10-26T09:30:00 +00:00")] // BST about to end
        [InlineData("2025-10-26T01:00:00 +00:00", "2025-10-26T09:30:00 +00:00")] // BST ended
        [InlineData("2025-10-26T01:00:01 +00:00", "2025-10-26T09:30:00 +00:00")] // One second after
        [InlineData("2025-10-26T02:00:00 +00:00", "2025-10-26T09:30:00 +00:00")] // One hour after
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DaylightSavingTimeEnding_FromIsUtc(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);

            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Theory]
        [InlineData("2025-10-26T01:00:00 +01:00", "2025-10-26T09:30:00 +00:00")] // One hour before BST ends
        [InlineData("2025-10-26T01:59:59 +01:00", "2025-10-26T09:30:00 +00:00")] // BST about to end
        [InlineData("2025-10-26T01:00:00 +00:00", "2025-10-26T09:30:00 +00:00")] // BST ended
        [InlineData("2025-10-26T01:00:01 +00:00", "2025-10-26T09:30:00 +00:00")] // One second after
        [InlineData("2025-10-26T02:00:00 +00:00", "2025-10-26T09:30:00 +00:00")] // One hour after
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkLondonTimezone_DaylightSavingTimeEnding_FromIsLocalTime(
            string from,
            string expectedNextOccurrence)
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var fromDateTime = DateTimeOffset.Parse(from);
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse(expectedNextOccurrence);
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, ukTimeZone);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Fact]
        public void GetNextOccurrence_HandlesCronExpressionWithSecondsPrecision()
        {
            const string cronExpression = "0 30 9 * * *"; // At 09:30:00 every day
            var fromDateTime = DateTimeOffset.Parse("2025-01-01T17:00:00Z");
            var expectedNextOccurrenceDateTime = DateTimeOffset.Parse("2025-01-02T09:30:00Z");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, TimeZoneInfo.Utc);

            Assert.Equal(expectedNextOccurrenceDateTime, result);
        }

        [Fact]
        public void GetNextOccurrence_ThrowsException_ForInvalidCronExpression()
        {
            const string cronExpression = "* * * *"; // Not enough fields
            var fromDateTime = DateTimeOffset.Parse("2025-01-01T12:00:00Z");

            Assert.Throws<Cronos.CronFormatException>(() =>
                CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, TimeZoneInfo.Utc));
        }
    }
}
