using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string ReleaseName { get; set; }

        /**
         * The last date the release was published - this should be set when the PublishScheduled date is reached and
         * the release is published.
         */
        public DateTime? Published { get; set; }

        // The date that the release is scheduled to be published - when this time is reached then the release should
        // be published and the Published date set.
        public DateTime? PublishScheduled { get; set; }

        [NotMapped]
        public bool Live => Published.HasValue && (DateTime.Compare(UtcNow, Published.Value) > 0);

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }
        
        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }
        
        public DataBlock KeyStatistics { get; set; }

        public Guid? TypeId { get; set; }

        public ReleaseType Type { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        public int Order { get; set; }

        protected bool Equals(Release other)
        {
            return Id.Equals(other.Id);
        }
        
        public bool IsLatestRelease(Release release)
        {
            return !release.Publication.Releases.Exists(
                r => r.Order > release.Order ||
                     (r.Id != release.Id && r.Order == release.Order && r.Published > release.Published));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Release) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}