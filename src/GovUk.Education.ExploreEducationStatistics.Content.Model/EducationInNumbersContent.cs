#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinContentSection
{
    public Guid Id { get; set; }
    public int Order { get; set; }

    [MaxLength(255)]
    public string Heading { get; set; } = string.Empty;
    public Guid EducationInNumbersPageId { get; set; }
    public EducationInNumbersPage EducationInNumbersPage { get; set; } = null!;
    public List<EinContentBlock> Content { get; set; } = [];
}

public abstract class EinContentBlock
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public Guid EinContentSectionId { get; set; }
    public EinContentSection EinContentSection { get; set; } = null!;
}

public enum EinBlockType
{
    // NOTE: Update ContentDbContext.ConfigureEinContentBlock if you add a new type!
    HtmlBlock,
    TileGroupBlock,
}

public class EinHtmlBlock : EinContentBlock
{
    public string Body { get; set; } = string.Empty;
}

public class EinTileGroupBlock : EinContentBlock
{
    [MaxLength(1024)]
    public string? Title { get; set; }
    public List<EinTile> Tiles { get; set; } = [];
}

public class EinTile
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid EinParentBlockId { get; set; }
    public EinTileGroupBlock EinParentBlock { get; set; } = null!;
}

public enum EinTileType
{
    // NOTE: Update ContentDbContext.ConfigureEinTile if you add a new type!
    FreeTextStatTile,
    ApiQueryStatTile,
}

public class EinFreeTextStatTile : EinTile
{
    public string Statistic { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
}

public class EinApiQueryStatTile : EinTile
{
    public Guid? DataSetId { get; set; }

    public string Version { get; set; } = string.Empty; // @MarkFix can the user provide a version without a patch version?

    public bool IsLatestVersion { get; set; } // @MarkFix change to string latestPublishedVersion

    public string Query { get; set; } = string.Empty;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<IndicatorUnit>))]
    public IndicatorUnit? IndicatorUnit { get; set; } = null;

    public int? DecimalPlaces { get; set; }

    public string QueryResult { get; set; } = string.Empty;

    public string MetaResult { get; set; } = string.Empty; // @MarkFix update migration to add
}

// @MarkFix filters results by NAT and latest TimeIdentifier/TimePeriod and then should have one result
// @MarkFix link to release that the api data set is from (like free stat text tile)
// @MarkFix on a new release being published, check for api data sets - if it is in an EinTile, update isLatestVersion AND any previous version of the tile in case of amendments!
