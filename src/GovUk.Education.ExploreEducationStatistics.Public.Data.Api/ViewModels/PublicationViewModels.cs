using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides summary information about a publication.
/// </summary>
public record PublicationSummaryViewModel
{
    /// <summary>
    /// The ID of the publication.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the publication.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The URL slug of the publication.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// The summary of the publication.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// When the publication was last published.
    /// </summary>
    public required DateTimeOffset LastPublished { get; init; }
}

/// <summary>
/// A paginated list of publication summaries.
/// </summary>
public record PublicationPaginatedListViewModel : PaginatedListViewModel<PublicationSummaryViewModel>;
