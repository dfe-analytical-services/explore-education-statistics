using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseFileReference
    {
        public Guid Id { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }
        
        public Guid? SubjectId { get; set; }
        
        public string Filename { get; set; }
    }
}