using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class EducationInNumbersContentViewModels
{
    public class EinContentSectionViewModel
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
                Content = section
                    .Content.Select(EinContentBlockViewModel.FromModel)
                    .OrderBy(block => block.Order)
                    .ToList(),
            };
        }
    }

    public class EinContentBlockViewModel
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

    public class EinHtmlBlockViewModel : EinContentBlockViewModel
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

    public class EinTileGroupBlockViewModel : EinContentBlockViewModel
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

    public class EinTileViewModel
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

    public class EinFreeTextStatTileViewModel : EinTileViewModel
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
                Type = EinTileType.FreeTextStatTile,
                Order = statTile.Order,
                Title = statTile.Title,
                Statistic = statTile.Statistic,
                Trend = statTile.Trend,
                LinkUrl = statTile.LinkUrl,
                LinkText = statTile.LinkText,
            };
        }
    }

    // @MarkFix just copied from Admin, so probably needs loads of changes
    public class EinApiQueryStatTileViewModel : EinTileViewModel
    {
        public string Title { get; init; } = string.Empty;
        public Guid? DataSetId { get; init; }
        public string Version { get; init; } = string.Empty;
        public bool IsLatestVersion { get; init; }
        public string Query { get; init; } = string.Empty;
        public IndicatorUnit? IndicatorUnit { get; init; } // @MarkFix needs a converter?
        public int? DecimalPlaces { get; set; }
        public string QueryResult { get; set; } = string.Empty;

        public static EinApiQueryStatTileViewModel FromModel(EinApiQueryStatTile statTile)
        {
            return new EinApiQueryStatTileViewModel
            {
                Id = statTile.Id,
                Order = statTile.Order,
                Type = EinTileType.ApiQueryStatTile,
                Title = statTile.Title,
                DataSetId = statTile.DataSetId,
                Version = statTile.Version,
                IsLatestVersion = statTile.IsLatestVersion,
                Query = statTile.Query,
                IndicatorUnit = statTile.IndicatorUnit,
                DecimalPlaces = statTile.DecimalPlaces,
                QueryResult = statTile.QueryResult,
            };
        }
    }
}
