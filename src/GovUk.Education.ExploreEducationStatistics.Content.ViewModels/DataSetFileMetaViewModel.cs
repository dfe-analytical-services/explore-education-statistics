namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileMetaViewModel
{
    public required List<string> GeographicLevels { get; init; }
    public required DataSetFileTimePeriodRangeViewModel TimePeriodRange { get; init; }
    public required List<string> Filters { get; init; }
    public required List<string> Indicators { get; init; }
}

public record DataSetFileTimePeriodRangeViewModel
{
    public required string From { get; init; }
    public required string To { get; init; }
}
