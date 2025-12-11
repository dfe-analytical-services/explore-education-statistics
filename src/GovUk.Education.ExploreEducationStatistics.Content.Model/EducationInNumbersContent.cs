#nullable enable
using System.ComponentModel.DataAnnotations;
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

    public Guid DataSetVersionId { get; set; }

    public SemVersion Version { get; set; } = null!;

    public string Query { get; set; } = string.Empty;

    // @MarkFix do we need the whole meta query result, or just the indicator type (i.e. num decimal places etc.)
    public string MetaQueryResult { get; set; } = string.Empty;

    public string QueryResult { get; set; } = string.Empty;

    public bool PreviousYearComparison { get; set; } = false;
}

// @MarkFix Public API query for EiN page
// - query must return multiple results
// -- results for previous year for comparison
// -- results for multiple locations if they want a chart displaying with other locations
// -- presumably some kind of warning if the query doesn't return these? Or if there is more than one result per location?
// -- we default to returning to NAT for location?
//
// - also need meta query to properly display results
//
// - can display one stat, a table, or even a chart
// -- one stat for now, but needs to be prepared for tables/charts
//
// - what happens if an api data set gets updated?
// -- cache the results, update the results once a day or something?
// -- what if the query fails after the api data sets updates
// -- what if the latest year results have changed - need the latest and previous years
// -- or maybe just some kind of alert to update the EiN page?
//
// MVP
// - add dataSetVersionId and query json body for Papi stat
// - filters by NAT
// -- gets the results for the latest year
// -- checks there is one result

// Post MVP
// - if Tile's new bool PreviousYearComparison == true, checks there is one result for the previous year
// - auto update EiN page when new api data set version is published (just do it when the new release is published?)
