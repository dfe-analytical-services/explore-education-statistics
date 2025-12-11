#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using Semver;

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
    public int Order { get; set; }
    public Guid EinParentBlockId { get; set; }
    public EinTileGroupBlock EinParentBlock { get; set; } = null!;
}

public enum EinTileType
{
    // NOTE: Update ContentDbContext.ConfigureEinTile if you add a new type!
    FreeTextStatTile,
    ApiQueryTile,
}

public class EinFreeTextStatTile : EinTile
{
    public string Title { get; set; } = string.Empty;
    public string Statistic { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
}

public class EinApiQueryStatTile : EinTile
{
    public string Title { get; set; } = string.Empty;

    public Guid DataSetId { get; set; }

    public string Version { get; set; } = string.Empty;

    public bool IsLatestVersion { get; set; }

    public string Query { get; set; } = string.Empty;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<IndicatorUnit>))]
    public IndicatorUnit IndicatorUnit { get; set; } = IndicatorUnit.None;

    public int? DecimalPlaces { get; set; }

    public string QueryResult { get; set; } = string.Empty;
}

// @MarkFix Public API papiStatTile for MVP - i.e. one stat drawn from a papi query
// MVP
// - filters results by NAT and latest TimeIdentifier/TimePeriod and then should have one result
// -- BAU must manually update the papi stat tile for the tile and then publish it - no auto update
// - link to release that the api data set is from (like free stat text tile)
