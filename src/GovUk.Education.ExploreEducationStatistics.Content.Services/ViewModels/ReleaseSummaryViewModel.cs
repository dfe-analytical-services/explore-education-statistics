#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record ReleaseSummaryViewModel
    {
        public Guid Id { get; }

        public string Title { get; }

        public string Slug { get; }

        public string YearTitle { get; }

        public string CoverageTitle { get; }

        public DateTime? Published { get; }

        public string ReleaseName { get; }

        public PartialDate NextReleaseDate { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; }

        public bool LatestRelease { get; }

        public bool IsSuperseded { get; set; }

        public DateTime? DataLastPublished { get; }

        public PublicationSummaryViewModel? Publication { get; }

        public ReleaseSummaryViewModel(CachedReleaseViewModel release, PublicationViewModel publication)
        {
            Id = release.Id;
            Title = release.Title;
            Slug = release.Slug;
            YearTitle = release.YearTitle;
            CoverageTitle = release.CoverageTitle;
            Published = release.Published;
            ReleaseName = release.ReleaseName;
            NextReleaseDate = release.NextReleaseDate;
            Type = ReleaseViewModel.ReleaseTypeTitleMap[release.Type.Title];
            LatestRelease = Id == publication.LatestReleaseId;
            DataLastPublished = release.DataLastPublished;
            Publication = new PublicationSummaryViewModel(publication);
        }

        public ReleaseSummaryViewModel(Release release)
        {
            Id = release.Id;
            Title = release.Title;
            Slug = release.Slug;
            YearTitle = release.YearTitle;
            CoverageTitle = release.TimePeriodCoverage.GetEnumLabel();
            Published = release.Published;
            ReleaseName = release.ReleaseName;
            NextReleaseDate = release.NextReleaseDate;
            Type = release.Type;
            LatestRelease = Id == release.Publication.LatestPublishedRelease()?.Id;
            DataLastPublished = release.DataLastPublished;
        }
    }
}
