using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

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
}
