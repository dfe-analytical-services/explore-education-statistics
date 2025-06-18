#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class ScreenerTestResultViewModel
{
    public required string TestFunctionName { get; set; }

    public required string Result { get; set; }

    public string? Notes { get; set; }

    public required string Stage { get; set; }
}
