using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumValueJsonConverter<GeographicLevel>>))]
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
/// A filter that groups related filterable options about the data (excluding geography or time).
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
    /// The filter options belonging to this filter.
    /// </summary>
    public required IReadOnlyList<FilterOptionMetaViewModel> Options { get; init; }
}

/// <summary>
/// A filterable option about the data.
/// </summary>
public record FilterOptionMetaViewModel
{
    /// <summary>
    /// The ID of the filter option.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label describing the filter option.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Whether the filter option is an aggregate (i.e. ‘all’ or a ‘total’) of the other filter options.
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
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Level { get; init; }

    /// <summary>
    /// The locations belonging to this level.
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
    /// The code of the location.
    /// </summary>
    public required string Code { get; init; }
}

public record LocationLocalAuthorityOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The ONS code of the local authority.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// </summary>
    public required string OldCode { get; init; }
}

public record LocationProviderOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// </summary>
    public required string Ukprn { get; init; }
}

public record LocationRscRegionOptionMetaViewModel : LocationOptionMetaViewModel
{
}

public record LocationSchoolOptionMetaViewModel : LocationOptionMetaViewModel
{
    /// <summary>
    /// The URN (unique reference number) of the school.
    /// </summary>
    public required string Urn { get; init; }

    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// </summary>
    public required string LaEstab { get; init; }
}

/// <summary>
/// A representation of a time period including a human-readable label.
/// </summary>
public record TimePeriodMetaViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public TimeIdentifier? Code { get; init; }

    /// <summary>
    /// The period that the time period relates to.
    /// </summary>
    public required string Period { get; init; }

    /// <summary>
    /// The time period in human-readable format.
    /// </summary>
    public required string Label { get; init; }
}
