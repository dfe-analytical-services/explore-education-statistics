using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class ReleaseViewModel : CachedReleaseViewModel
    {
        public ReleaseViewModel(CachedReleaseViewModel release,
            CachedPublicationViewModel publication) : base(
            release.Id,
            release.Title,
            release.YearTitle,
            release.CoverageTitle,
            release.ReleaseName,
            release.Published,
            release.Slug,
            release.Type,
            release.Updates,
            release.Content,
            release.SummarySection,
            release.HeadlinesSection,
            release.KeyStatisticsSection,
            release.KeyStatisticsSecondarySection,
            release.DownloadFiles,
            release.RelatedInformation)
        {
            Publication = new PublicationViewModel(
                publication.Id,
                publication.Title,
                publication.Slug,
                publication.Description,
                publication.DataSource,
                publication.Summary,
                publication.NextUpdate,
                publication.LatestReleaseId,
                // TODO should be releases excluding this one, not releases
                publication.Releases,
                publication.LegacyReleases,
                publication.Topic,
                publication.Contact,
                publication.ExternalMethodology,
                publication.Methodology);
        }

        public bool LatestRelease => Id == Publication.LatestReleaseId;

        public PublicationViewModel Publication { get; }
    }
}