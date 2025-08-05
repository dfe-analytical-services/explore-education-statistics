namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetFileMetaViewModel
{
    public required int NumDataFileRows { get; set; }
    public required List<string> GeographicLevels { get; set; }
    public required DataSetFileTimePeriodRangeViewModel TimePeriodRange { get; init; }
    public required List<string> Filters { get; init; }
    public required List<string> Indicators { get; init; }
}

public record DataSetFileCsvPreviewViewModel
{
    public List<string> Headers { get; init; } = new();
    public List<List<string>> Rows { get; init; } = new();
}

public record DataSetFileTimePeriodRangeViewModel
{
    public required string From { get; init; }
    public required string To { get; init; }
}
