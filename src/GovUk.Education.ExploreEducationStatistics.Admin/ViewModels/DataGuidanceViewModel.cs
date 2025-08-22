#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataGuidanceViewModel
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<DataGuidanceDataSetViewModel> DataSets { get; init; } = new();
}
