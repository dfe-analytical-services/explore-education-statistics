using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// All the metadata associated with a data set.
/// </summary>
public record DataSetMetaViewModel
{
    /// <summary>
    /// All the filters associated with the data set.
    /// </summary>
    public required IReadOnlyList<FilterOptionsViewModel> Filters { get; init; }

    /// <summary>
    /// All the indicators associated with the data set.
    /// </summary>
    public required IReadOnlyList<IndicatorViewModel> Indicators { get; init; }

    /// <summary>
    /// All the geographic levels associated with the data set.
    /// </summary>
    public required IReadOnlyList<GeographicLevelViewModel> GeographicLevels { get; init; }

    /// <summary>
    /// All the locations associated with the data set, grouped by geographic level.
    /// </summary>
    public required IReadOnlyList<LocationGroupOptionsViewModel> Locations { get; init; } // @MarkFix THIS IS THE PROBLEM

    /// <summary>
    /// All the time periods associated with the data set.
    /// </summary>
    public required IReadOnlyList<TimePeriodOptionViewModel> TimePeriods { get; init; }
}

/// <summary>
/// A type of data point measured by a data set.
/// </summary>
public record IndicatorViewModel
{
    /// <summary>
    /// The ID of the indicator.
    /// </summary>
    /// <example>enW68</example>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the indicator CSV column.
    /// </summary>
    /// <example>sess_authorised</example>
    public required string Column { get; init; }

    /// <summary>
    /// The human-readable label of the indicator.
    /// </summary>
    /// <example>Percentage of authorised sessions</example>
    public required string Label { get; init; }

    /// <summary>
    /// The type of unit that should be used when formatting the indicator.
    /// </summary>
    /// <example>%</example>
    [JsonConverter(typeof(EnumToEnumLabelJsonConverter<IndicatorUnit>))]
    public required IndicatorUnit? Unit { get; init; }

    /// <summary>
    /// The recommended number of decimal places to use when formatting the indicator.
    /// </summary>
    /// <example>2</example>
    public int? DecimalPlaces { get; init; }

    public static IndicatorViewModel Create(IndicatorMeta meta)
    {
        return new IndicatorViewModel
        {
            Id = meta.PublicId,
            Column = meta.Column,
            Label = meta.Label,
            Unit = meta.Unit,
            DecimalPlaces = meta.DecimalPlaces,
        };
    }
}

/// <summary>
/// A geographic level (e.g. national, regional) covered by a data set.
/// </summary>
public record GeographicLevelViewModel
{
    /// <summary>
    /// The code for the geographic level.
    /// </summary>
    /// <example>NAT</example>
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Code { get; init; }

    /// <summary>
    /// The human-readable label for the geographic level.
    /// </summary>
    /// <example>National</example>
    public string Label => Code.GetEnumLabel();

    public static GeographicLevelViewModel Create(GeographicLevel level)
    {
        return new GeographicLevelViewModel { Code = level };
    }
}

/// <summary>
/// The options available for a filterable characteristic about the data set
/// (excluding geography or time).
/// </summary>
public record FilterOptionsViewModel : FilterViewModel
{
    /// <summary>
    /// The filter options belonging to this filter.
    /// </summary>
    [JsonPropertyOrder(1)]
    public required IReadOnlyList<FilterOptionViewModel> Options { get; init; }
}

/// <summary>
/// The options available for a location group in the data set.
/// </summary>
public record LocationGroupOptionsViewModel : LocationGroupViewModel
{
    /// <summary>
    /// The locations belonging to this level.
    /// </summary>
    [JsonPropertyOrder(1)]
    public required IReadOnlyList<LocationOptionViewModel> Options { get; init; }
}

/// <summary>
/// A time period option that can be used to filter a data set.
/// </summary>
public record TimePeriodOptionViewModel : TimePeriodViewModel
{
    /// <summary>
    /// The time period in human-readable format.
    /// </summary>
    /// <example>2020/21 Q1</example>
    [JsonPropertyOrder(1)]
    public required string Label { get; init; }

    public static TimePeriodOptionViewModel Create(TimePeriodMeta meta)
    {
        return new TimePeriodOptionViewModel
        {
            Code = meta.Code,
            Period = meta.Period,
            Label = TimePeriodFormatter.FormatLabel(meta.Period, meta.Code),
        };
    }
}
