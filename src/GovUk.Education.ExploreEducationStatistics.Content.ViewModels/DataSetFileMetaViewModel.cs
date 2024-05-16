namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileMetaViewModel
{
    public List<string> GeographicLevels { get; init; } = new();

    public DataSetFileTimePeriodRangeViewModel TimePeriodRange { get; init; } = null!;
    public List<string> Filters { get; init; } = new();
    public List<string> Indicators { get; init; } = new();
}

public record DataSetFileTimePeriodRangeViewModel
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
}
