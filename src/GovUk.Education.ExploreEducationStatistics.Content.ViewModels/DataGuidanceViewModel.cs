using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataGuidanceViewModel : ReleaseSummaryViewModel
{
    public required string DataGuidance { get; init; }

    public required List<DataGuidanceDataSetViewModel> DataSets { get; init; }

    [SetsRequiredMembers]
    public DataGuidanceViewModel(
        ReleaseCacheViewModel release,
        PublicationCacheViewModel publication,
        List<DataGuidanceDataSetViewModel> dataSets
    )
        : base(release, publication)
    {
        DataGuidance = release.DataGuidance;
        DataSets = dataSets;
    }
}
