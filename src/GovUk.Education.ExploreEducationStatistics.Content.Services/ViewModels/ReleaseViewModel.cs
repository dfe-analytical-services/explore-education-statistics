#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record ReleaseViewModel
    {
        public Guid Id { get; }

        public string Title { get; }

        public string YearTitle { get; }

        public string CoverageTitle { get; }

        public string ReleaseName { get; }

        public PartialDate? NextReleaseDate { get; }

        public DateTime? Published { get; }

        public string Slug { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; }

        public List<ReleaseNoteViewModel> Updates { get; }

        public List<ContentSectionViewModel> Content { get; }

        public ContentSectionViewModel? SummarySection { get; }

        public ContentSectionViewModel? HeadlinesSection { get; }

        public ContentSectionViewModel? KeyStatisticsSection { get; }

        public ContentSectionViewModel? KeyStatisticsSecondarySection { get; }

        public List<FileInfo> DownloadFiles { get; }

        public bool HasPreReleaseAccessList { get; }

        public bool HasDataGuidance => DownloadFiles.Any(file => file.Type == FileType.Data);

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
            Type = ReleaseTypeTitleMap[release.Type.Title];
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
                publication.LatestReleaseId,
                otherReleases,
                publication.LegacyReleases,
                publication.Topic,
                publication.Contact,
                publication.ExternalMethodology
            );
        }

        /// TODO EES-3127 Remove the backwards compatibility of CachedReleaseViewModel.Type.
        public static readonly Dictionary<string, ReleaseType> ReleaseTypeTitleMap = 
            EnumUtil.GetEnumValues<ReleaseType>()
            .ToDictionary(v => v.GetTitle(), v => v);

        public bool LatestRelease => Id == Publication.LatestReleaseId;

        public PublicationViewModel Publication { get; }
    }
}
