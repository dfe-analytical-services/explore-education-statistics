#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels
{
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
}
