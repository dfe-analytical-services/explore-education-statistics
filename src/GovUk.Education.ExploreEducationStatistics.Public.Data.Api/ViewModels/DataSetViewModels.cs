using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides high-level information about a data set.
/// </summary>
public record DataSetViewModel
{
    /// <summary>
    /// The ID of the data set. 
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the data set.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// A summary of the data set’s contents.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// The status of the data set. Can be one of the following:
    ///
    /// - `Published` - the data set has been published and will receive updates
    /// - `Deprecated` - the data set is being discontinued and will no receive updates
    /// - `Withdrawn` - the data set has been withdrawn and can no longer be used
    /// </summary>
    public required DataSetStatus Status { get; init; }

    /// <summary>
    /// The latest published data set version.
    /// </summary>
    public required DataSetLatestVersionViewModel LatestVersion { get; init; }

    /// <summary>
    /// The ID of the data set that supersedes this data set (if it has been deprecated).
    /// </summary>
    public Guid? SupersedingDataSetId { get; init; }
}

/// <summary>
/// Provides high-level information about the latest version of a data set.
/// </summary>
public record DataSetLatestVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor).
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumLabelJsonConverter<GeographicLevel>>))]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// The filters available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Filters { get; init; }

    /// <summary>
    /// The indicators available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }
}

/// <summary>
/// A paginated list of data sets.
/// </summary>
public record DataSetPaginatedListViewModel : PaginatedListViewModel<DataSetViewModel>;

public class DataSetVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor).
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// The version type. Can be one of the following:
    ///
    /// - `Major` - backwards incompatible changes are being introduced
    /// - `Minor` - backwards compatible changes are being introduced
    /// 
    /// Major versions typically indicate that some action may be required 
    /// to ensure code that consumes the data set continues to work.
    /// 
    /// Minor versions should not cause issues in the functionality of existing code.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    /// <summary>
    /// The version’s status. Can be one of the following:
    ///
    /// - `Published` - the version is published and can be used
    /// - `Deprecated` - the version is being deprecated and will not be usable in the future
    /// - `Withdrawn` - the version has been withdrawn and can no longer be used
    /// </summary>
    public required DataSetVersionStatus Status { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// When the version was withdrawn.
    /// </summary>
    public DateTimeOffset? Withdrawn { get; init; }

    /// <summary>
    /// Any notes about this version and its changes.
    /// </summary>
    public required string Notes { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumLabelJsonConverter<GeographicLevel>>))]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// The filters available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Filters { get; init; }

    /// <summary>
    /// The indicators available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }
}

/// <summary>
/// A paginated list of data set versions.
/// </summary>
public record DataSetVersionPaginatedListViewModel : PaginatedListViewModel<DataSetVersionViewModel>;

/// <summary>
/// All the metadata associated with a data set.
/// </summary>
public record DataSetMetaViewModel
{
    /// <summary>
    /// All the filters associated with the data set.
    /// </summary>
    public required IReadOnlyList<FilterMetaViewModel> Filters { get; init; }

    /// <summary>
    /// All the indicators associated with the data set.
    /// </summary>
    public required IReadOnlyList<IndicatorMetaViewModel> Indicators { get; init; }

    /// <summary>
    /// All the geographic levels associated with the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumLabelJsonConverter<GeographicLevel>>))]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// All the locations associated with the data set, grouped by level.
    /// </summary>
    public required IReadOnlyList<LocationLevelMetaViewModel> Locations { get; init; }

    /// <summary>
    /// All the time periods associated with the data set.
    /// </summary>
    public required IReadOnlyList<TimePeriodMetaViewModel> TimePeriods { get; init; }
}

/// <summary>
/// A group of filterable facets (or characteristics) for the data. This is composed of filter items.
/// </summary>
public record FilterMetaViewModel
{
    /// <summary>
    /// The ID of the filter.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// A hint to assist in describing the filter.
    /// </summary>
    public string Hint { get; init; } = string.Empty;

    /// <summary>
    /// The human-readable label describing the filter.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// The filter items belonging to this filter.
    /// </summary>
    public required IReadOnlyList<FilterOptionMetaViewModel> Options { get; init; }
}

/// <summary>
/// A filterable facet (or characteristic) of a data point.
/// </summary>
public record FilterOptionMetaViewModel
{
    /// <summary>
    /// The ID of the filter item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label describing the filter item.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Whether the filter item is an aggregate (i.e. ‘all’ or a ‘total’) of the other filter items.
    /// </summary>
    public bool? IsAggregate { get; init; }
}

/// <summary>
/// The type of measurement taken by a data point.
/// </summary>
public record IndicatorMetaViewModel
{
    /// <summary>
    /// The ID of the indicator.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label of the indicator.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// A numeric unit for an indicator.
    /// 
    /// Available options:
    /// - "
    /// - `%`
    /// - `£`
    /// - `£m`
    /// - `pp`
    /// </summary>
    [JsonConverter(typeof(EnumToEnumLabelJsonConverter<IndicatorUnit>))]
    public required IndicatorUnit? Unit { get; init; }

    /// <summary>
    /// The optimal number of decimal places that the indicator should use when displayed.
    /// </summary>
    public int? DecimalPlaces { get; init; }
}

/// <summary>
/// A geographic level’s locations.
/// </summary>
public record LocationLevelMetaViewModel
{
    /// <summary>
    /// A geographic level that locations are grouped by.
    /// 
    /// Available options:
    /// - `Country`
    /// - `EnglishDevolvedArea`
    /// - `LocalAuthority`
    /// - `LocalAuthorityDistrict`
    /// - `LocalEnterprisePartnership`
    /// - `Institution`
    /// - `MayoralCombinedAuthority`
    /// - `MultiAcademyTrust`
    /// - `OpportunityArea`
    /// - `ParliamentaryConstituency`
    /// - `PlanningArea`
    /// - `Provider`
    /// - `Region`
    /// - `RscRegion`
    /// - `School`
    /// - `Sponsor`
    /// - `Ward`
    /// </summary>
    [JsonConverter(typeof(EnumToEnumLabelJsonConverter<GeographicLevel>))]
    public required GeographicLevel Level { get; init; }

    /// <summary>
    /// The geographical locations belonging to this level.
    /// </summary>
    public required IReadOnlyList<LocationOptionMetaViewModel> Options { get; init; }
}

/// <summary>
/// A geographic location e.g. a country, region, local authority, etc.
/// </summary>
[JsonConverter(typeof(LocationOptionMetaJsonConverter))]
public abstract record LocationOptionMetaViewModel
{
    /// <summary>
    /// The ID of the location.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The label of the location.
    /// </summary>
    public required string Label { get; init; }
}

public record LocationCodedOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The code of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string Code { get; init; }
}

public record LocationLocalAuthorityOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The code of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The old code of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string OldCode { get; init; }
}

public record LocationProviderOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The ukprn of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string Ukprn { get; init; }
}

public record LocationRscRegionOptionMetaViewModel : LocationOptionMetaViewModel
{
}

public record LocationSchoolOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The urn of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string Urn { get; init; }

    /// <summary>
    /// The LaEstab of the location
    /// 
    /// Note that these codes may not be unique across locations at the same geographic level.
    /// </summary>
    public required string LaEstab { get; init; }
}

/// <summary>
/// A representation of a time period including a human-readable label.
/// </summary>
public record TimePeriodMetaViewModel
{
    /// <summary>
    /// The code describing a time period. This can be one of the following:
    /// - `AY` - Academic year
    /// - `AYQ1-4` - Academic year quarter 1 to 4
    /// - `CY` - Calendar year
    /// - `RY` - Reporting year
    /// - `P1` - Part 1 (April to September)
    /// - `P2` - Part 2 (October to March)
    /// - `FY` - Financial year
    /// - `FYQ1-4` - Financial year quarter 1 to 4
    /// - `TYQ1-4` - Tax year quarter 1 to 4
    /// - `W1-52` - Week 1 to 52
    /// - `M1-12` - Month 1 to 12
    /// 
    /// Available options:
    /// - `AY`
    /// - `AYQ1`
    /// - `AYQ2`
    /// - `AYQ3`
    /// - `AYQ4`
    /// - `CY`
    /// - `CYQ1`
    /// - `CYQ2`
    /// - `CYQ3`
    /// - `CYQ4`
    /// - `P1`
    /// - `P2`
    /// - `FY`
    /// - `FYQ1`
    /// - `FYQ2`
    /// - `FYQ3`
    /// - `FYQ4`
    /// - `TY`
    /// - `TYQ1`
    /// - `TYQ2`
    /// - `TYQ3`
    /// - `TYQ4`
    /// - `RY`
    /// - `T1`
    /// - `T1T2`
    /// - `T2`
    /// - `T3`
    /// - `W1`
    /// - `W2`
    /// - `W3`
    /// - `W4`
    /// - `W5`
    /// - `W6`
    /// - `W7`
    /// - `W8`
    /// - `W9`
    /// - `W10`
    /// - `W11`
    /// - `W12`
    /// - `W13`
    /// - `W14`
    /// - `W15`
    /// - `W16`
    /// - `W17`
    /// - `W18`
    /// - `W19`
    /// - `W20`
    /// - `W21`
    /// - `W22`
    /// - `W23`
    /// - `W24`
    /// - `W25`
    /// - `W26`
    /// - `W27`
    /// - `W28`
    /// - `W29`
    /// - `W30`
    /// - `W31`
    /// - `W32`
    /// - `W33`
    /// - `W34`
    /// - `W35`
    /// - `W36`
    /// - `W37`
    /// - `W38`
    /// - `W39`
    /// - `W40`
    /// - `W41`
    /// - `W42`
    /// - `W43`
    /// - `W44`
    /// - `W45`
    /// - `W46`
    /// - `W47`
    /// - `W48`
    /// - `W49`
    /// - `W50`
    /// - `W51`
    /// - `W52`
    /// - `M1`
    /// - `M2`
    /// - `M3`
    /// - `M4`
    /// - `M5`
    /// - `M6`
    /// - `M7`
    /// - `M8`
    /// - `M9`
    /// - `M10`
    /// - `M11`
    /// - `M12`
    /// </summary>
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public TimeIdentifier? Code { get; init; }

    /// <summary>
    /// The period that the time period relates to.
    /// </summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>
    /// The time period in human-readable format.
    /// </summary>
    public string Label { get; init; } = string.Empty;
}
