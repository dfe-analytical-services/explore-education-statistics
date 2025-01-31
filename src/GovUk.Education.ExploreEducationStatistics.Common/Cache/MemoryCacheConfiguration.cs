using NCrontab;

#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public record MemoryCacheConfiguration(int DurationInSeconds, CrontabSchedule? ExpirySchedule = null);