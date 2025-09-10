#nullable enable
using Cronos;
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
        [Theory]
        [MemberData(nameof(CronExpressionUtilTestsTheoryData.UtcTimeZoneTestData),
            MemberType = typeof(CronExpressionUtilTestsTheoryData))]
        [MemberData(nameof(CronExpressionUtilTestsTheoryData.UkTimeZoneTestData),
            MemberType = typeof(CronExpressionUtilTestsTheoryData))]
        public void GetNextOccurrence_ReturnsExpectedResult(
            DateTimeOffset from,
            DateTimeOffset expectedNextOccurrence,
            string cronExpression,
            TimeZoneInfo timezone,
            string significance)
        {
            Print($"Cron expression: '{cronExpression}'\n" +
                  $"From: {from}\n" +
                  $"Significance: '{significance}'\n" +
                  $"Time zone the next occurrence should be evaluated in: {timezone.Id}\n" +
                  $"Expected next occurrence: {expectedNextOccurrence}");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, from, timezone);

            Assert.NotNull(result);
            Assert.Equal(expectedNextOccurrence, result.Value);
        }

        [Fact]
        public void GetNextOccurrence_HandlesCronExpressionWithSecondPrecision()
        {
            const string cronExpression = "0 30 9 * * *"; // At 09:30:00 every day
            var from = DateTimeOffset.Parse("2025-01-01T09:30:00 +00:00");
            var expectedNextOccurrence = DateTimeOffset.Parse("2025-01-02T09:30:00 +00:00");

            var nextOccurrence = CronExpressionUtil.GetNextOccurrence(cronExpression, from, TimeZoneInfo.Utc);

            Assert.Equal(expectedNextOccurrence, nextOccurrence);
        }

        [Fact]
        public void GetNextOccurrence_ReturnsNull_ForCronExpressionWithNoNextOccurrence()
        {
            const string cronExpression = "0 0 30 2 *"; // 30th of February is unreachable
            var from = DateTimeOffset.Parse("2025-01-01T12:00:00 +00:00");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, from, TimeZoneInfo.Utc);

            Assert.Null(result);
        }

        [Fact]
        public void GetNextOccurrence_ThrowsException_ForInvalidCronExpression()
        {
            const string cronExpression = "* * * *"; // Not enough fields
            var from = DateTimeOffset.Parse("2025-01-01T12:00:00Z");

            Assert.Throws<CronFormatException>(() =>
                CronExpressionUtil.GetNextOccurrence(cronExpression, from, TimeZoneInfo.Utc));
        }
    }
}
