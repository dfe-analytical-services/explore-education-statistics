using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class PreReleaseAccessListViewModel : ReleaseSummaryViewModel
    {
        public string PreReleaseAccessList { get; }

        public PreReleaseAccessListViewModel(
            CachedReleaseViewModel release,
            CachedPublicationViewModel publication) : base(release, publication)
        {
            PreReleaseAccessList = release.PreReleaseAccessList;
        }
    }
}
