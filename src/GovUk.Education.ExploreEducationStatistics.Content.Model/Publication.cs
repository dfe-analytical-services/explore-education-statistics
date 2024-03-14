#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Publication
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)] public string Title { get; set; } = string.Empty;

        [MaxLength(160)]
        public string Summary { get; set; } = string.Empty;

        public List<ReleaseVersion> ReleaseVersions { get; set; } = new();

        public List<PublicationMethodology> Methodologies { get; set; } = new();

        public ExternalMethodology? ExternalMethodology { get; set; }

        public List<LegacyRelease> LegacyReleases { get; set; } = new();

        public Guid TopicId { get; set; }

        public Topic Topic { get; set; } = null!;

        public Guid ContactId { get; set; }

        public Contact Contact { get; set; } = null!;

        public Guid? SupersededById { get; set; }

        public Publication? SupersededBy { get; set; }

        public DateTime? Updated { get; set; }

        public bool Live => LatestPublishedReleaseVersionId.HasValue;

        public Guid? LatestPublishedReleaseVersionId { get; set; }

        public ReleaseVersion? LatestPublishedReleaseVersion { get; set; }

        public List<ReleaseSeriesItem> ReleaseSeriesView { get; set; } = new();
    }
}
