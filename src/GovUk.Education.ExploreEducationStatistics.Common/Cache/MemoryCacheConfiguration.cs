using Cronos;

#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public record MemoryCacheConfiguration(int DurationInSeconds, CronExpression? ExpirySchedule = null);