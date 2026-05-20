using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;

public record ScreenerProgressViewModel
{
    public DataSetUploadScreeningStatus Status { get; set; }

    public int PercentageComplete { get; set; }

    public string Stage { get; set; } = null!;

    public bool Completed { get; set; }
}
