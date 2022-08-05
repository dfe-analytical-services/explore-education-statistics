using Cronos;

#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public static class ExpirySchedules
{
    public const string Hourly = "0 * * * *";
    public const string HalfHourly = "*/30 * * * *";
}

public record MemoryCacheConfiguration(int CacheDurationInSeconds, CronExpression? ExpirySchedule = null);