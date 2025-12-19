namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublishedPublicationSummaryViewModel
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public DateTimeOffset Published { get; init; }
}
