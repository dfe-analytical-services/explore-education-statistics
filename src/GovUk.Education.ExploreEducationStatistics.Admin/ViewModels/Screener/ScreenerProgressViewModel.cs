namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;

public record ScreenerProgressViewModel
{
    public int PercentageComplete { get; set; }

    public string Stage { get; set; } = null!;
}

public record ScreenerProgressWithDataSetUploadIdViewModel : ScreenerProgressViewModel
{
    public Guid DataSetUploadId { get; set; }
}
