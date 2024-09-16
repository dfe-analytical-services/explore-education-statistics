using System.Text.Json.Serialization;
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

    /// <summary>
    /// Whether the filter option is an aggregate (i.e. ‘all’ or a ‘total’) of the other filter options.
    /// </summary>
    /// <example>false</example>
    public bool? IsAggregate { get; init; }

    public static FilterOptionViewModel Create(FilterOptionMetaChange.State changeState)
    {
        return new FilterOptionViewModel
        {
            Id = changeState.PublicId,
            Label = changeState.Option.Label,
            IsAggregate = changeState.Option.IsAggregate
        };
    }
}
