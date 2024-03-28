namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseVersionTitleViewModel
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
