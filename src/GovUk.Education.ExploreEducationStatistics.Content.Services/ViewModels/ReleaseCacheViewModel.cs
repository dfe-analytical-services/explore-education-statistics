#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record ReleaseCacheViewModel
    {
        public ReleaseCacheViewModel(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string YearTitle { get; set; } = string.Empty;

        public string CoverageTitle { get; set; } = string.Empty;

        public string ReleaseName { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        public PartialDate? NextReleaseDate { get; set; }

        public DateTime? Published { get; set; }

        public List<ReleaseNoteViewModel> Updates { get; set; } = new();

        public List<ContentSectionViewModel> Content { get; set; } = new();

        public ContentSectionViewModel SummarySection { get; set; } = null!;

        public ContentSectionViewModel HeadlinesSection { get; set; } = null!;

        public List<KeyStatisticViewModel> KeyStatistics { get; set; } = new();

        public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; } = null!;

        public ContentSectionViewModel? RelatedDashboardsSection { get; set; }

        public List<FileInfo> DownloadFiles { get; set; } = new();

        public string DataGuidance { get; set; } = string.Empty;

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public List<LinkViewModel> RelatedInformation { get; set; } = new();
    }
}
