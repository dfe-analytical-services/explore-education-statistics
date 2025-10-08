using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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
