using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class MetaGuidanceViewModel : ReleaseSummaryViewModel
    {
        public string MetaGuidance { get;  }

        public List<MetaGuidanceSubjectViewModel> Subjects { get; }

        public MetaGuidanceViewModel(
            CachedReleaseViewModel release,
            CachedPublicationViewModel publication,
            List<MetaGuidanceSubjectViewModel> subjects) : base(release, publication)
        {
            MetaGuidance = release.MetaGuidance;
            Subjects = subjects;
        }
    }
}
