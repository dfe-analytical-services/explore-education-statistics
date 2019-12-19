using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleaseStatusViewModel
    {
        public Guid ReleaseId { get; set; }
        public string ContentStage { get; set; }
        public string FilesStage { get; set; }
        public string DataStage { get; set; }
        public string Stage { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}