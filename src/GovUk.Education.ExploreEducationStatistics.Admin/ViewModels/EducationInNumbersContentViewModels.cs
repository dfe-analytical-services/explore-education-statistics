#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record EinContentViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; } = string.Empty;

    public DateTimeOffset? Published { get; set; }

    public List<EinContentSectionViewModel> Content { get; set; } = [];

    public static EinContentViewModel FromModel(EducationInNumbersPage page)
    {
        return new EinContentViewModel
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Published = page.Published,
            Content = page
                .Content.Select(EinContentSectionViewModel.FromModel)
                .OrderBy(section => section.Order)
                .ToList(),
        };
    }
}

public record EinContentSectionViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Heading { get; set; } = string.Empty;

    public List<EinContentBlockViewModel> Content { get; set; } = new();

    public static EinContentSectionViewModel FromModel(EinContentSection section)
    {
        return new EinContentSectionViewModel
        {
            Id = section.Id,
            Order = section.Order,
            Heading = section.Heading,
            Content = section.Content.Select(EinContentBlockViewModel.FromModel).OrderBy(block => block.Order).ToList(),
        };
    }
}

public record EinContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public EinBlockType Type { get; set; }

    public static EinContentBlockViewModel FromModel(EinContentBlock block)
    {
        return block switch
        {
            EinHtmlBlock htmlBlock => EinHtmlBlockViewModel.FromModel(htmlBlock),
            EinTileGroupBlock groupBlock => EinTileGroupBlockViewModel.FromModel(groupBlock),
            _ => throw new Exception($"{nameof(EinContentBlock)} type {block.GetType()} not found"),
        };
    }
}

public record EinHtmlBlockViewModel : EinContentBlockViewModel
{
    public string Body { get; set; } = string.Empty;

    public static EinHtmlBlockViewModel FromModel(EinHtmlBlock htmlBlock)
    {
        return new EinHtmlBlockViewModel
        {
            Id = htmlBlock.Id,
            Order = htmlBlock.Order,
            Type = EinBlockType.HtmlBlock,
            Body = htmlBlock.Body,
        };
    }
}

public record EinTileGroupBlockViewModel : EinContentBlockViewModel
{
    public string? Title { get; set; }
    public List<EinTileViewModel> Tiles { get; set; } = new();

    public static EinTileGroupBlockViewModel FromModel(EinTileGroupBlock groupBlock)
    {
        return new EinTileGroupBlockViewModel
        {
            Id = groupBlock.Id,
            Order = groupBlock.Order,
            Type = EinBlockType.TileGroupBlock,
            Title = groupBlock.Title,
            Tiles = groupBlock.Tiles.Select(EinTileViewModel.FromModel).OrderBy(tile => tile.Order).ToList(),
        };
    }
}

public record EinTileViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public EinTileType Type { get; set; }

    public static EinTileViewModel FromModel(EinTile tile)
    {
        return tile switch
        {
            EinFreeTextStatTile statTile => EinFreeTextStatTileViewModel.FromModel(statTile),
            EinApiQueryStatTile statTile => EinApiQueryStatTileViewModel.FromModel(statTile),
            _ => throw new Exception($"{nameof(EinTile)} type {tile.GetType()} not found"),
        };
    }
}

public record EinFreeTextStatTileViewModel : EinTileViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Statistic { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }

    public static EinFreeTextStatTileViewModel FromModel(EinFreeTextStatTile statTile)
    {
        return new EinFreeTextStatTileViewModel
        {
            Id = statTile.Id,
            Order = statTile.Order,
            Type = EinTileType.FreeTextStatTile,
            Title = statTile.Title,
            Statistic = statTile.Statistic,
            Trend = statTile.Trend,
            LinkUrl = statTile.LinkUrl,
            LinkText = statTile.LinkText,
        };
    }
}

public record EinApiQueryStatTileViewModel : EinTileViewModel
{
    public string Title { get; init; } = string.Empty;
    public Guid? DataSetId { get; init; }
    public string Version { get; init; } = string.Empty;
    public string LatestPublishedVersion { get; init; } = string.Empty;
    public string Stat { get; init; }
    public string Query { get; init; } = string.Empty;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<IndicatorUnit>))]
    public IndicatorUnit? IndicatorUnit { get; init; }

    public int? DecimalPlaces { get; set; }
    public string QueryResult { get; set; } = string.Empty; // @MarkFix temp?
    public string MetaResult { get; set; } = string.Empty; // @MarkFix temp?

    public static EinApiQueryStatTileViewModel FromModel(EinApiQueryStatTile statTile)
    {
        return new EinApiQueryStatTileViewModel // @MarkFix validate this is all correct / as desired
        {
            Id = statTile.Id,
            Order = statTile.Order,
            Type = EinTileType.ApiQueryStatTile,
            Title = statTile.Title,
            DataSetId = statTile.DataSetId,
            Version = statTile.Version,
            LatestPublishedVersion = statTile.LatestPublishedVersion,
            Stat = statTile.Statistic,
            Query = statTile.Query,
            IndicatorUnit = statTile.IndicatorUnit,
            DecimalPlaces = statTile.DecimalPlaces,
            QueryResult = statTile.QueryResult,
            MetaResult = statTile.MetaResult,
        };
    }
}
