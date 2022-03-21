﻿#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public record CachedReleaseViewModel
    {
        public CachedReleaseViewModel(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string YearTitle { get; set; } = string.Empty;

        public string CoverageTitle { get; set; } = string.Empty;

        public string ReleaseName { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        // TODO EES-3127 Replace ReleaseTypeViewModel with ReleaseType. Requires a content cache refresh
        public ReleaseTypeViewModel Type { get; set; } = null!;

        public PartialDate? NextReleaseDate { get; set; }

        public DateTime? Published { get; set; }

        public List<ReleaseNoteViewModel> Updates { get; set; } = new();

        public List<ContentSectionViewModel> Content { get; set; } = new();

        public ContentSectionViewModel SummarySection { get; set; } = null!;

        public ContentSectionViewModel HeadlinesSection { get; set; } = null!;

        public ContentSectionViewModel KeyStatisticsSection { get; set; } = null!;

        public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; } = null!;

        public List<FileInfo> DownloadFiles { get; set; } = new();

        public string DataGuidance { get; set; } = string.Empty;

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public List<LinkViewModel> RelatedInformation { get; set; } = new();

        private DateTime? _dataLastPublished;

        public DateTime? DataLastPublished
        {
            get => _dataLastPublished;
            set => _dataLastPublished = value ?? DateTime.UtcNow;
        }
    }
}
