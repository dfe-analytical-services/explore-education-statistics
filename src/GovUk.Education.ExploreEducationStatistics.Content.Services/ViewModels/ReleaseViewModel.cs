#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

        public List<KeyStatisticViewModel> KeyStatistics { get; }

        public ContentSectionViewModel? KeyStatisticsSecondarySection { get; }

        public ContentSectionViewModel? RelatedDashboardsSection { get; }

        public List<FileInfo> DownloadFiles { get; }

        public bool HasPreReleaseAccessList { get; }

        public bool HasDataGuidance => DownloadFiles.Any(file => file.Type == FileType.Data);

        public List<LinkViewModel> RelatedInformation { get; }

        public PublicationViewModel Publication { get; }

        public ReleaseViewModel(
            ReleaseCacheViewModel release,
            PublicationViewModel publication)
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
            KeyStatistics = release.KeyStatistics;
            KeyStatisticsSecondarySection = release.KeyStatisticsSecondarySection;
            RelatedDashboardsSection = release.RelatedDashboardsSection;
            DownloadFiles = release.DownloadFiles;
            HasPreReleaseAccessList = !release.PreReleaseAccessList.IsNullOrEmpty();
            RelatedInformation = release.RelatedInformation;
            Publication = publication;
        }

        public bool LatestRelease => Id == Publication.LatestReleaseId;
    }
}
