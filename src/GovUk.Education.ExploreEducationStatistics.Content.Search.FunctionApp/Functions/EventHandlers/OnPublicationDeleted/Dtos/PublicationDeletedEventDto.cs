namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationDeleted.Dtos;

public record PublicationDeletedEventDto
{
    public string? PublicationSlug { get; init; }

    public IReadOnlyList<Guid>? ReleaseIds { get; init; }
}
