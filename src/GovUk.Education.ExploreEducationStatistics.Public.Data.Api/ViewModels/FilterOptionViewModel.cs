using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A filterable option that can be used to filter a data set.
/// </summary>
public record FilterOptionViewModel
{
    /// <summary>
    /// The ID of the filter option.
    /// </summary>
    /// <example>q1g3J</example>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label describing the filter option.
    /// </summary>
    /// <example>State-funded primary</example>
    public required string Label { get; init; }

    public static FilterOptionViewModel Create(FilterOptionMetaChange.State changeState)
    {
        return new FilterOptionViewModel
        {
            Id = changeState.PublicId,
            Label = changeState.Option.Label,
        };
    }
}
