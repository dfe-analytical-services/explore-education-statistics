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
        [Fact]
        public void GetNextOccurrence_ReturnsCorrectNextOccurrence_ForValidCronExpression()
        {
            const string cronExpression = "30 9 * * *"; // At 09:30 every day
            var baseTime = DateTimeOffset.Parse("2025-01-01T12:00:00Z");
            var expectedNextOccurrence = DateTimeOffset.Parse("2025-01-02T09:30:00Z");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, baseTime);

            Assert.Equal(expectedNextOccurrence, result);
        }

        [Fact]
        public void GetNextOccurrence_HandlesCronExpressionWithSecondsPrecision()
        {
            const string cronExpression = "0 30 9 * * *"; // At 09:30:00 every day
            var baseTime = DateTimeOffset.Parse("2025-01-01T12:00:00Z");
            var expectedNextOccurrence = DateTimeOffset.Parse("2025-01-02T09:30:00Z");

            var result = CronExpressionUtil.GetNextOccurrence(cronExpression, baseTime);

            Assert.Equal(expectedNextOccurrence, result);
        }

        [Fact]
        public void GetNextOccurrence_ThrowsException_ForInvalidCronExpression()
        {
            const string cronExpression = "* * * *"; // Not enough fields
            var baseTime = DateTimeOffset.Parse("2025-01-01T12:00:00Z");

            Assert.Throws<NCrontab.CrontabException>(() =>
                CronExpressionUtil.GetNextOccurrence(cronExpression, baseTime));
        }
    }
}
