namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A group of locations in a data set based on their geographic level.
/// </summary>
public record LocationGroupViewModel
{
    /// <summary>
    /// The geographic level of the locations in this group.
    /// </summary>
    /// <example>NAT</example>
    public required GeographicLevelViewModel Level { get; init; }
}
