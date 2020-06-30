using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject
    {
        public Subject Subject { get; set; }
        
        public Guid SubjectId { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }
    }
}