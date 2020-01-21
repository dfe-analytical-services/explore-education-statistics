using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleaseStatusViewModel
    {
        public Guid ReleaseId { get; set; }
        public string DataStage { get; set; }
        public string ContentStage { get; set; }
        public string FilesStage { get; set; }
        public string PublishingStage { get; set; }
        public string OverallStage { get; set; }
        public IEnumerable<ReleaseStatusLogMessage> LogMessages { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}