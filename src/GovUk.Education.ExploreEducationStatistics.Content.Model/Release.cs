using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string ReleaseName { get; set; }

        public DateTime? Published { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }
        
        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }
        
        public DataBlock KeyStatistics { get; set; }

        public Guid TypeId { get; set; }

        public ReleaseType Type { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        protected bool Equals(Release other)
        {
            return Id.Equals(other.Id);
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