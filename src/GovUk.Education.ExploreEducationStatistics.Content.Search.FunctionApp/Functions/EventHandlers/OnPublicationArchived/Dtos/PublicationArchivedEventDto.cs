namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationArchived.Dtos;

public class PublicationArchivedEventDto
{
    public Guid? SupersededByPublicationId { get; init; }

    public string? PublicationSlug { get; init; }
}
