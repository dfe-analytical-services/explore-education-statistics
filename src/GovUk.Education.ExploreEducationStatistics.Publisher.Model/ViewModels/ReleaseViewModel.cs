namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class ReleaseViewModel : CachedReleaseViewModel
    {
        public ReleaseViewModel(CachedReleaseViewModel cachedReleaseViewModel,
            PublicationViewModel publication) : base(cachedReleaseViewModel.Id,
            cachedReleaseViewModel.Title,
            cachedReleaseViewModel.YearTitle,
            cachedReleaseViewModel.CoverageTitle,
            cachedReleaseViewModel.ReleaseName,
            cachedReleaseViewModel.Published,
            cachedReleaseViewModel.Slug,
            cachedReleaseViewModel.Type,
            cachedReleaseViewModel.Updates,
            cachedReleaseViewModel.Content,
            cachedReleaseViewModel.SummarySection,
            cachedReleaseViewModel.HeadlinesSection,
            cachedReleaseViewModel.KeyStatisticsSection,
            cachedReleaseViewModel.KeyStatisticsSecondarySection,
            cachedReleaseViewModel.DownloadFiles,
            cachedReleaseViewModel.RelatedInformation)
        {
            Publication = publication;
            LatestRelease = cachedReleaseViewModel.Id == publication.LatestReleaseId;
        }

        public PublicationViewModel Publication { get; }

        public bool LatestRelease { get; }
    }
}