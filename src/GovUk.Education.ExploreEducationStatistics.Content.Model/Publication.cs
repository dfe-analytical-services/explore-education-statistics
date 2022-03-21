#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Publication
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)] public string Title { get; set; } = string.Empty;

        public List<Release> Releases { get; set; } = new();

        public List<PublicationMethodology> Methodologies { get; set; } = new();

        public ExternalMethodology? ExternalMethodology { get; set; }

        public Uri? LegacyPublicationUrl { get; set; }

        public List<LegacyRelease> LegacyReleases { get; set; } = new();

        public DateTime? Published { get; set; }

        public Guid TopicId { get; set; }

        public Topic Topic { get; set; } = null!;

        public Guid? ContactId { get; set; }

        public Contact Contact { get; set; } = null!;

        public DateTime? Updated { get; set; }

        public bool Live => Published.HasValue && DateTime.Compare(DateTime.UtcNow, Published.Value) > 0;

        public Release? LatestPublishedRelease()
        {
            return Releases
                .Where(IsLatestPublishedVersionOfRelease)
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .LastOrDefault();
        }

        public Release? LatestRelease()
        {
            return Releases
                .Where(r => IsLatestVersionOfRelease(r.Id))
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .LastOrDefault();
        }

        public bool IsLatestVersionOfRelease(Guid releaseId)
        {
            return !Releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }

        public List<Release> ListActiveReleases()
        {
            return Releases
                .Where(r => IsLatestVersionOfRelease(r.Id))
                .ToList();
        }

        private bool IsLatestPublishedVersionOfRelease(Release release)
        {
            return
                // Release itself must be live
                release.Live
                // It must also be the latest version unless the later version is a draft
                && !Releases.Any(r =>
                    r.Live
                    && r.PreviousVersionId == release.Id
                    && r.Id != release.Id);
        }
    }
}
