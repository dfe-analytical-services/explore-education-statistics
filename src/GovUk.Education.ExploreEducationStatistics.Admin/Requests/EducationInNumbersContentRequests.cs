#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record EinContentSectionAddRequest
{
    public int Order { get; set; }
}

public record EinContentSectionUpdateHeadingRequest
{
    public string Heading { get; set; } = string.Empty;
}

public record EinContentBlockAddRequest
{
    public EinBlockType Type { get; set; }

    public int? Order { get; set; }
}

public record EinHtmlBlockUpdateRequest
{
    public string Body { get; set; } = string.Empty;
}

public record EinTileGroupBlockUpdateRequest
{
    public string? Title { get; set; }
}

public record EinTileAddRequest
{
    public EinTileType Type { get; set; }

    public int? Order { get; set; }
}

public record EinFreeTextStatTileUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Statistic { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
}

