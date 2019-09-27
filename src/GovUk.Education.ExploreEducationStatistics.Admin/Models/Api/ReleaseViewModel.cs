using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; }
        public string ReleaseName { get; set; }
        
        public string CoverageTitle { get; set; }
        
        public string YearTitle { get; set; }

        public Guid? TypeId { get; set; }

        public PartialDate NextReleaseDate { get; set; }

        public DateTime? PublishScheduled { get; set; }

        public DateTime? Published { get; set; }

        public bool Live => Published != null;

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier? TimePeriodCoverage { get; set; }

        public bool LatestRelease { get; set; }
        
        public ReleaseType Type { get; set; }
        
        public Contact Contact { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus Status { get; set; }

        public string InternalReleaseNote { get; set; }
        
        public List<Comment> DraftComments { get; set; }

        public List<Comment> HigherReviewComments { get; set; }

        public class Comment
        {
            public string AuthorName { get; set; }
            
            public DateTime CreatedDate { get; set; }

            public string Message { get; set; }
        }
    }
}
