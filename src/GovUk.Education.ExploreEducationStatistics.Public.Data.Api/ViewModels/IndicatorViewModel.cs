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

    public static IndicatorViewModel Create(IndicatorMeta meta)
    {
        return new IndicatorViewModel
        {
            Id = meta.PublicId,
            Label = meta.Label,
            Unit = meta.Unit,
            DecimalPlaces = meta.DecimalPlaces
        };
    }
}