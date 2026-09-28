using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;

public record PublicationDeletedEventDto
{
    public string? PublicationSlug { get; init; }

    public LatestPublishedReleaseInfo? LatestPublishedRelease { get; init; }

    /// <summary>
    /// The ids of every Release that belonged to the deleted Publication.
    /// </summary>
    /// <remarks>
    /// Absent from events raised before this field was added, which is why LatestPublishedRelease is still
    /// honoured alongside it.
    /// </remarks>
    public IReadOnlyList<Guid>? ReleaseIds { get; init; }
}
