namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record MethodologyVersionSummaryViewModel
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
