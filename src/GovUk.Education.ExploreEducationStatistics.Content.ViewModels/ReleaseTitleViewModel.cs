namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseTitleViewModel
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }
}
