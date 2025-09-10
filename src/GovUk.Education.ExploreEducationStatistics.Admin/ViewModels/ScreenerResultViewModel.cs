#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ScreenerResultViewModel
{
    public required string OverallResult { get; set; }

    public required string Message { get; set; }

    public List<ScreenerTestResultViewModel> TestResults { get; set; } = [];
}
