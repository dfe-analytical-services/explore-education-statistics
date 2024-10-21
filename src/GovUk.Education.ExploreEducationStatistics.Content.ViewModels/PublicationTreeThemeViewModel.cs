namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationTreeThemeViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public List<PublicationTreePublicationViewModel> Publications { get; init; } = [];
}
