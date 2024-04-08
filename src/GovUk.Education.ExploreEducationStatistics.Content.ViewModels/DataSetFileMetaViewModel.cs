namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileMetaViewModel
{
    public List<string> GeographicLevels { get; init; } = new();
    public DataSetFileTimePeriodViewModel TimePeriod { get; init; } = new();
    public List<string> Filters { get; init; } = new();
    public List<string> Indicators { get; init; } = new();
}

public record DataSetFileTimePeriodViewModel
{
    public string TimeIdentifier { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
}
