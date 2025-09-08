using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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

    public static LocationGroupViewModel Create(LocationMeta meta)
    {
        return new LocationGroupViewModel { Level = GeographicLevelViewModel.Create(meta.Level) };
    }
}
