namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record OrganisationViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }
}
