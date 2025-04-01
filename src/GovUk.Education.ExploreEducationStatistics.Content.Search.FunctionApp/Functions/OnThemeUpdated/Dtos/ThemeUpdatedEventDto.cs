namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.OnThemeUpdated.Dtos;

public class ThemeUpdatedEventDto
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Slug { get; init; }
}
