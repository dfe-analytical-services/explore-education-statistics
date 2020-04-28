using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject
    {
        public Subject Subject { get; set; }
        
        public Guid SubjectId { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }

        public ReleaseSubject CreateReleaseAmendment(Release amendment)
        {
            var copy = MemberwiseClone() as ReleaseSubject;
            copy.Release = amendment;
            copy.ReleaseId = amendment.Id;
            return copy;
        }
    }
}