using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides summary information about a publication.
/// </summary>
public record PublicationSummaryViewModel
{
    /// <summary>
    /// The ID of the publication.
    /// </summary>
    /// <example>d851c09e-7f5a-4750-9191-ed67ba5e8f8b</example>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the publication.
    /// </summary>
    /// <example>Pupil absence in schools in England</example>
    public required string Title { get; init; }

    /// <summary>
    /// The URL slug of the publication.
    /// </summary>
    /// <example>pupil-absence-in-schools-in-england</example>
    public required string Slug { get; init; }

    /// <summary>
    /// The summary of the publication.
    /// </summary>
    /// <example>Summary of the publication.</example>
    public required string Summary { get; init; }

    /// <summary>
    /// When the publication was last published.
    /// </summary>
    /// <example>2024-06-01T09:30:00+00:00</example>
    public required DateTimeOffset LastPublished { get; init; }
}

/// <summary>
/// A paginated list of publication summaries.
/// </summary>
public record PublicationPaginatedListViewModel : PaginatedListViewModel<PublicationSummaryViewModel>;
