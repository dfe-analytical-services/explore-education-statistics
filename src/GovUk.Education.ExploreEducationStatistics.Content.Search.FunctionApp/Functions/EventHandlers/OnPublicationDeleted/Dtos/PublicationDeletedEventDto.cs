using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;

public record PublicationDeletedEventDto
{
    public string? PublicationSlug { get; init; }

    public LatestPublishedReleaseInfo? LatestPublishedRelease { get; init; }
}
