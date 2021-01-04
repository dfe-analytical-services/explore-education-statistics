using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Publication
    {
        public Guid Id { get; set; }

        public string Slug { get; set; }

        [Required] public string Title { get; set; }

        public string Description { get; set; }

        public string DataSource { get; set; }

        public string Summary { get; set; }

        public List<Release> Releases { get; set; } = new List<Release>();

        public Guid? MethodologyId { get; set; }

        public Methodology Methodology { get; set; }

        public ExternalMethodology ExternalMethodology { get; set; }

        public Uri LegacyPublicationUrl { get; set; }

        public List<LegacyRelease> LegacyReleases { get; set; }

        public DateTime? Published { get; set; }

        public Guid TopicId { get; set; }

        public Topic Topic { get; set; }

        public Guid? ContactId { get; set; }

        public Contact Contact { get; set; }

        public DateTime? Updated { get; set; }

        public bool Live => Published.HasValue && DateTime.Compare(DateTime.UtcNow, Published.Value) > 0;

        public Release LatestPublishedRelease()
        {
            return Releases?.Where(r => r.Live && IsLatestVersionOfRelease(r.Id))
                .OrderBy(r => r.Year)
                .ThenBy(r => r.TimePeriodCoverage)
                .LastOrDefault();
        }

        private bool IsLatestVersionOfRelease(Guid releaseId)
        {
            return !Releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}