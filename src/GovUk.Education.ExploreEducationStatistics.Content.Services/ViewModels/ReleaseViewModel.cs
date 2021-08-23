using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class ReleaseViewModel
    {
        public Guid Id { get; }

        public string Title { get; }

        public string YearTitle { get; }

        public string CoverageTitle { get; }

        public string ReleaseName { get; }

        public PartialDate NextReleaseDate { get; }

        public DateTime? Published { get; }

        public string Slug { get; }

        public ReleaseTypeViewModel Type { get; }

        public List<ReleaseNoteViewModel> Updates { get; }

        public List<ContentSectionViewModel> Content { get; }

        public ContentSectionViewModel SummarySection { get; }

        public ContentSectionViewModel HeadlinesSection { get; }

        public ContentSectionViewModel KeyStatisticsSection { get; }

        public ContentSectionViewModel KeyStatisticsSecondarySection { get; }

        public List<FileInfo> DownloadFiles { get; }

        public bool HasPreReleaseAccessList { get; }

        public bool HasMetaGuidance => DownloadFiles.Any(file => file.Type == FileType.Data);

        public List<LinkViewModel> RelatedInformation { get; }

        public DateTime? DataLastPublished { get; }

        public ReleaseViewModel(
            CachedReleaseViewModel release,
            CachedPublicationViewModel publication)
        {
            Id = release.Id;
            Title = release.Title;
            YearTitle = release.YearTitle;
            CoverageTitle = release.CoverageTitle;
            ReleaseName = release.ReleaseName;
            NextReleaseDate = release.NextReleaseDate;
            Published = release.Published;
            Slug = release.Slug;
            Type = release.Type;
            Updates = release.Updates;
            Content = release.Content;
            SummarySection = release.SummarySection;
            HeadlinesSection = release.HeadlinesSection;
            KeyStatisticsSection = release.KeyStatisticsSection;
            KeyStatisticsSecondarySection = release.KeyStatisticsSecondarySection;
            DownloadFiles = release.DownloadFiles;
            HasPreReleaseAccessList = !release.PreReleaseAccessList.IsNullOrEmpty();
            RelatedInformation = release.RelatedInformation;
            DataLastPublished = release.DataLastPublished;

            var otherReleases = publication.Releases
                .Where(model => Id != model.Id)
                .ToList();

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
                publication.ExternalMethodology
            );
        }

        public bool LatestRelease => Id == Publication.LatestReleaseId;

        public PublicationViewModel Publication { get; }
    }
}
