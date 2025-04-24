#nullable enable
using System;
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CronExpressionUtil
{
    public static DateTimeOffset GetNextOccurrence(
        string cronExpression,
        DateTimeOffset baseTime)
    {
        return CrontabSchedule.Parse(cronExpression,
                new CrontabSchedule.ParseOptions
                {
                    IncludingSeconds = CronExpressionHasSecondPrecision(cronExpression)
                })
            .GetNextOccurrence(baseTime: baseTime.UtcDateTime);
    }

    public static bool CronExpressionHasSecondPrecision(string cronExpression)
    {
        return cronExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length == 6;
    }
}
