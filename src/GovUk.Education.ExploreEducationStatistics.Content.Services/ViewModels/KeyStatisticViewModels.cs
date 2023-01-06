#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public abstract class KeyStatisticViewModel
{
    public Guid Id { get; set; }

    public Guid ReleaseId { get; set; }

    public string? Trend { get; set; } = string.Empty;

    public string? GuidanceTitle { get; set; } = string.Empty;

    public string? GuidanceText { get; set; } = string.Empty;

    public int Order { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}

public class KeyStatisticDataBlockViewModel : KeyStatisticViewModel
{
    public Guid DataBlockId { get; set; }
}

public class KeyStatisticTextViewModel : KeyStatisticViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Statistic { get; set; } = string.Empty;
}
