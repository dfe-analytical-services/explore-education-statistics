#nullable enable
using System;
using Cronos;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CronExpressionUtil
{
    /// <summary>
    /// Gets the next occurrence of a Cron expression from a starting point in time, in the specified time zone.
    /// Wraps <see cref="CronExpression.GetNextOccurrence"/> to handle Cron expressions with potential second precision.
    /// </summary>
    /// <param name="cronExpression">The Cron expression to evaluate.</param>
    /// <param name="from">The starting point in time to calculate the next occurrence, which can be in any offset,
    /// not necessarily the offset of the given time zone.</param>
    /// <param name="timeZoneInfo">The time zone which the next occurrence should be evaluated in.</param>
    /// <returns>
    /// The next occurrence of the Cron expression as a <see cref="DateTimeOffset"/>, or <c>null</c> if no future occurrence exists.
    /// </returns>
    public static DateTimeOffset? GetNextOccurrence(
        string cronExpression,
        DateTimeOffset from,
        TimeZoneInfo timeZoneInfo)
    {
        var expression = CronExpression.Parse(cronExpression,
            CronExpressionHasSecondPrecision(cronExpression) ? CronFormat.IncludeSeconds : CronFormat.Standard);

        return expression.GetNextOccurrence(from, timeZoneInfo);
    }

    /// <summary>
    /// Determines whether a Cron expression includes second-level precision.
    /// </summary>
    /// <param name="cronExpression">The Cron expression to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the Cron expression includes seconds; otherwise, <c>false</c>.
    /// </returns>
    public static bool CronExpressionHasSecondPrecision(string cronExpression)
    {
        return cronExpression.Split([' '], StringSplitOptions.RemoveEmptyEntries).Length == 6;
    }
}
