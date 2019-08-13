using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseSummary : AbstractVersioned<ReleaseSummaryVersion>
    {
        public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }
        
        public Guid Release { get; set; }
    }
}