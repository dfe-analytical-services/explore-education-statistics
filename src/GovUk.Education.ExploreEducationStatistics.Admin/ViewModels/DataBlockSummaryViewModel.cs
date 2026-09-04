#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataBlockSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Heading { get; init; }

    public required string Name { get; init; }

    public DateTime? Created { get; init; }

    public string? HighlightName { get; init; }

    public string? HighlightDescription { get; init; }

    public string? Source { get; init; }

    public required string DataSetName { get; init; }

    public required bool InContent { get; init; }

    public required int ChartsCount { get; init; }
}
