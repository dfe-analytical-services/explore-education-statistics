using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string ReleaseName { get; set; }
        
        public DateTime Published { get; set; }

        public DateTime PublishScheduled { get; set; }

        public bool Live { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }
        
        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }
        
        public DataBlock KeyStatistics { get; set; }

        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        public int Order { get; set; }
    }
}