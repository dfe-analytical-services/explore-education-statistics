#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record KeyStatisticDataBlockCreateRequest
{
    public Guid DataBlockId { get; set; }

    [MaxLength(KeyStatistic.TrendMaxLength)]
    public string? Trend { get; set; }

    [MaxLength(KeyStatistic.GuidanceTitleMaxLength)]
    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }
}

public record KeyStatisticDataBlockUpdateRequest
{
    [MaxLength(KeyStatistic.TrendMaxLength)]
    public string? Trend { get; set; }

    [MaxLength(KeyStatistic.GuidanceTitleMaxLength)]
    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }
}

public abstract record KeyStatisticTextSaveRequest
{
    [MaxLength(KeyStatistic.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(KeyStatistic.StatisticMaxLength)]
    public string Statistic { get; set; } = string.Empty;

    [MaxLength(KeyStatistic.TrendMaxLength)]
    public string? Trend { get; set; }

    [MaxLength(KeyStatistic.GuidanceTitleMaxLength)]
    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }
}

public record KeyStatisticTextCreateRequest : KeyStatisticTextSaveRequest;
public record KeyStatisticTextUpdateRequest : KeyStatisticTextSaveRequest;
