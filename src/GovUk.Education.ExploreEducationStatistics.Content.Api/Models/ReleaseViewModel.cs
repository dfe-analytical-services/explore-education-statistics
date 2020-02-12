using System.Linq;
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
            release.NextReleaseDate,
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
            var otherReleases = publication.Releases.Where(model => Id != model.Id).ToList();
            Publication = new PublicationViewModel(
                publication.Id,
                publication.Title,
                publication.Slug,
                publication.Description,
                publication.DataSource,
                publication.Summary,
                publication.LatestReleaseId,
                otherReleases,
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