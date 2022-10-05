#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PreReleaseAccessListViewModel : ReleaseSummaryViewModel
    {
        public string PreReleaseAccessList { get; }

        public PreReleaseAccessListViewModel(
            ReleaseCacheViewModel release,
            PublicationCacheViewModel publication) : base(release, publication)
        {
            PreReleaseAccessList = release.PreReleaseAccessList;
        }
    }
}
