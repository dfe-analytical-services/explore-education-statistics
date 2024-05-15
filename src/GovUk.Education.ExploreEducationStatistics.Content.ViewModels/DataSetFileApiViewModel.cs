namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileApiViewModel
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }
}
