#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public abstract class KeyStatistic : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public const int TitleMaxLength = 60;
    public const int StatisticMaxLength = 12;
    public const int TrendMaxLength = 230;
    public const int GuidanceTitleMaxLength = 65;

    public Guid Id { get; set; }

    public Guid ReleaseId { get; set; }

    public Release Release { get; set; } = null!;

    public string? Trend { get; set; }

    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }

    public int Order { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public Guid? UpdatedById { get; set; }

    public User? UpdatedBy { get; set; }

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

    public DataBlock DataBlock { get; set; } = null!;
}

public class KeyStatisticText : KeyStatistic
{
    public string Title { get; set; } = string.Empty;

    public string Statistic { get; set; } = string.Empty;
}
