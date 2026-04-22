#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;

public record ScreenerResultViewModel
{
    public required string OverallResult { get; set; }

    public List<ScreenerTestResultViewModel> TestResults { get; set; } = [];
}
