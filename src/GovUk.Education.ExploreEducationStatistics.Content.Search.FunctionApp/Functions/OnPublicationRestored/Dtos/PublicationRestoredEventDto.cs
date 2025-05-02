namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnPublicationRestored.Dtos;

public class PublicationRestoredEventDto
{
    public Guid? PreviousSupersededByPublicationId { get; init; }

    public string? PublicationSlug { get; init; }
}
