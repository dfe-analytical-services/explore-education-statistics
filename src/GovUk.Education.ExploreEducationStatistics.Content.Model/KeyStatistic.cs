#nullable enable
using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public abstract class KeyStatistic : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public Guid ReleaseId { get; set; }

    [IgnoreMap] public Release Release { get; set; } = null!;

    public string? Trend { get; set; } = string.Empty;

    public string? GuidanceTitle { get; set; } = string.Empty;

    public string? GuidanceText { get; set; } = string.Empty;

    public int Order { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public Guid ContentBlockIdTemp { get; set; } // TODO: Remove in EES-3988

    public KeyStatistic Clone(Release newRelease)
    {
        var copy = MemberwiseClone() as KeyStatistic;
        copy.Id = Guid.NewGuid();
        copy.Release = newRelease;
        copy.ReleaseId = newRelease.Id;

        return copy;
    }
}

public class KeyStatisticDataBlock : KeyStatistic
{
    public Guid DataBlockId { get; set; }

    [IgnoreMap] public DataBlock DataBlock { get; set; } = null!;
}

public class KeyStatisticText : KeyStatistic
{
    public string Title { get; set; } = string.Empty;

    public string Statistic { get; set; } = string.Empty;
}
