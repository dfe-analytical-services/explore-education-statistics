using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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
