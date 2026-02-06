#nullable enable
using System.Text.Json;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
    public string Title { get; init; } = string.Empty;
    public string Statistic { get; init; } = string.Empty;
    public string Trend { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public string? LinkText { get; init; }

    public class Validator : AbstractValidator<EinFreeTextStatTileUpdateRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Title).NotEmpty().MaximumLength(2048);
            RuleFor(r => r.Statistic).NotEmpty();
            RuleFor(x => x.LinkUrl).IsValidUrl().When(r => r.LinkUrl != null && r.LinkText != null);
            RuleFor(x => x.LinkText).MinimumLength(1).When(r => r.LinkText != null && r.LinkUrl != null);
        }
    }
}

public record EinApiQueryStatTileUpdateRequest
{
    public string Title { get; init; } = string.Empty;
    public Guid DataSetId { get; init; }
    public string Version { get; init; } = string.Empty;
    public string Query { get; init; } = string.Empty;

    public class Validator : AbstractValidator<EinApiQueryStatTileUpdateRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Title).NotEmpty().MaximumLength(2048);
            RuleFor(r => r.Version).NotEmpty().MaximumLength(32);
            RuleFor(r => r.Query).NotEmpty().IsValidJson();
        }
    }
}
