#nullable enable
using System;
using Cronos;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CronExpressionUtil
{
    public static DateTimeOffset? GetNextOccurrence(
        string cronExpression,
        DateTimeOffset from,
        TimeZoneInfo timeZoneInfo)
    {
        var expression = CronExpression.Parse(cronExpression,
            CronExpressionHasSecondPrecision(cronExpression) ? CronFormat.IncludeSeconds : CronFormat.Standard);

        return expression.GetNextOccurrence(from, timeZoneInfo);
    }

    public static bool CronExpressionHasSecondPrecision(string cronExpression)
    {
        return cronExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length == 6;
    }
}
