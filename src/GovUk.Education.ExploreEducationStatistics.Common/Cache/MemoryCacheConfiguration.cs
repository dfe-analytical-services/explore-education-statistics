#nullable enable
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache;

public record MemoryCacheConfiguration(
    int DurationInSeconds,
    CrontabSchedule? ExpirySchedule = null
);
