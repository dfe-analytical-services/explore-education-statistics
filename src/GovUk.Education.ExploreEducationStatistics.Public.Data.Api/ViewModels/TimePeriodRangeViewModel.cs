namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Describes a time period range in human-readable format.
/// </summary>
public record TimePeriodRangeViewModel
{
    /// <summary>
    /// The starting time period in human-readable format.
    /// </summary>
    public required string Start { get; set; }

    /// <summary>
    /// The ending time period in human-readable format.
    /// </summary>
    public required string End { get; set; }
}
