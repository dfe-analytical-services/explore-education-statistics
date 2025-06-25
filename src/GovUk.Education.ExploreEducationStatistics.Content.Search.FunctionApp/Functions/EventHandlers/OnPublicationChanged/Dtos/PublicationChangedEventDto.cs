namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationChanged.Dtos;

public class PublicationChangedEventDto
{
    public string? Title { get; init; }
    public string? Summary { get; init; }
    public string? Slug { get; init; }
    public bool? IsPublicationArchived { get; init; }
}
