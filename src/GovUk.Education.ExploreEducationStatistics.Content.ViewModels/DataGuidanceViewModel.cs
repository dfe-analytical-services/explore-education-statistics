using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataGuidanceViewModel : ReleaseSummaryViewModel
{
    public string DataGuidance { get;  }

    public List<DataGuidanceDataSetViewModel> DataSets { get; }

    public DataGuidanceViewModel(
        ReleaseCacheViewModel release,
        PublicationCacheViewModel publication,
        List<DataGuidanceDataSetViewModel> dataSets) : base(release, publication)
    {
        DataGuidance = release.DataGuidance;
        DataSets = dataSets;
    }
}