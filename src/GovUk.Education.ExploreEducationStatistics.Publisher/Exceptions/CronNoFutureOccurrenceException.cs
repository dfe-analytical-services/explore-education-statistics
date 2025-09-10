namespace GovUk.Education.ExploreEducationStatistics.Publisher.Exceptions;

/// <summary>
/// Exception thrown when there is no future occurrence for a given Cron expression.
/// </summary>
/// <param name="cronExpression">The Cron expression which was evaluated.</param>
/// <param name="from">The starting point in time which the next occurrence was calculated from.</param>
/// <param name="timeZone">The time zone which the next occurrence was evaluated in.</param>
public class CronNoFutureOccurrenceException(
    string cronExpression,
    DateTimeOffset from,
    TimeZoneInfo timeZone) : Exception(
    $"No next occurrence for Cron expression: '{cronExpression}' from: '{from}' evaluated in time zone: '${timeZone.Id}'");
