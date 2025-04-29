#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class CronExpressionUtilTests(ITestOutputHelper output)
{
    private void Print(string s) => output.WriteLine(s);

    public class CronExpressionHasSecondPrecisionTests(ITestOutputHelper output) : CronExpressionUtilTests(output)
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

    public class GetNextOccurrenceTests(ITestOutputHelper output) : CronExpressionUtilTests(output)
    {
        private readonly TimeZoneInfo _ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

        [Theory]
        [MemberData(nameof(CronExpressionUtilTestsTheoryData.UtcTimeZoneTestData),
            MemberType = typeof(CronExpressionUtilTestsTheoryData))]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUtcTimezone_FromIsUtc(
            string from,
            string expectedNextOccurrence,
            string cronExpression,
            string significance)
        {
            var (parsedFrom, parsedExpectedNextOccurrence) =
                CronExpressionUtilTestsTheoryData.ParseTheoryData(from, expectedNextOccurrence);

            // Convert from into UTC if it is not already
            var fromDateTimeOffsetUtc = parsedFrom.ToUniversalTime();

            AssertNextOccurrence(
                cronExpression,
                from: fromDateTimeOffsetUtc,
                expectedNextOccurrence: parsedExpectedNextOccurrence,
                timeZoneInfo: TimeZoneInfo.Utc,
                significance);
        }

        [Theory]
        [MemberData(nameof(CronExpressionUtilTestsTheoryData.UkTimeZoneTestData),
            MemberType = typeof(CronExpressionUtilTestsTheoryData))]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkTimezone_FromIsUtc(
            string from,
            string expectedNextOccurrence,
            string cronExpression,
            string significance)
        {
            var (parsedFrom, parsedExpectedNextOccurrence) =
                CronExpressionUtilTestsTheoryData.ParseTheoryData(from, expectedNextOccurrence);

            // Convert from into UTC if it is not already
            var fromUtc = parsedFrom.ToUniversalTime();

            AssertNextOccurrence(
                cronExpression,
                from: fromUtc,
                expectedNextOccurrence: parsedExpectedNextOccurrence,
                timeZoneInfo: _ukTimeZone,
                significance);
        }

        [Theory]
        [MemberData(nameof(CronExpressionUtilTestsTheoryData.UkTimeZoneTestData),
            MemberType = typeof(CronExpressionUtilTestsTheoryData))]
        public void GetNextOccurrence_ReturnsExpectedResult_ForUkTimezone_FromIsUkTime(
            string from,
            string expectedNextOccurrence,
            string cronExpression,
            string significance)
        {
            var (parsedFrom, parsedExpectedNextOccurrence) =
                CronExpressionUtilTestsTheoryData.ParseTheoryData(from, expectedNextOccurrence);

            // Convert from into UK time if it is not already
            var fromUkTime = TimeZoneInfo.ConvertTime(parsedFrom, _ukTimeZone);

            AssertNextOccurrence(
                cronExpression,
                from: fromUkTime,
                expectedNextOccurrence: parsedExpectedNextOccurrence,
                timeZoneInfo: _ukTimeZone,
                significance);
        }

        [Fact]
        public void GetNextOccurrence_HandlesCronExpressionWithSecondPrecision()
        {
            const string cronExpression = "0 30 9 * * *"; // At 09:30:00 every day

            AssertNextOccurrence(
                cronExpression,
                from: DateTimeOffset.Parse("2025-01-01T17:00:00Z"),
                expectedNextOccurrence: DateTimeOffset.Parse("2025-01-02T09:30:00Z"),
                timeZoneInfo: TimeZoneInfo.Utc);
        }

        [Fact]
        public void GetNextOccurrence_ThrowsException_ForInvalidCronExpression()
        {
            const string cronExpression = "* * * *"; // Not enough fields
            var fromDateTime = DateTimeOffset.Parse("2025-01-01T12:00:00Z");

            Assert.Throws<Cronos.CronFormatException>(() =>
                CronExpressionUtil.GetNextOccurrence(cronExpression, fromDateTime, TimeZoneInfo.Utc));
        }

        private void AssertNextOccurrence(
            string cronExpression,
            DateTimeOffset from,
            DateTimeOffset expectedNextOccurrence,
            TimeZoneInfo timeZoneInfo,
            string? significance = null)
        {
            Print($"Cron expression: '{cronExpression}'\n" +
                  $"From: {from}\n" +
                  (significance != null ? $"Significance: '{significance}'\n" : "") +
                  $"Time zone: {timeZoneInfo.Id}\n" +
                  $"Expected next occurrence: {expectedNextOccurrence}");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, from, timeZoneInfo);

            Assert.NotNull(result);
            // There's no need to convert the expected and actual values to use the same offset.
            // Assert.Equal compares expected and actual values accounting for their respective offsets.
            Assert.Equal(expectedNextOccurrence, result.Value);
        }
    }
}
