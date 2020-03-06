using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public abstract class Versioned<TSelf>
    {
        public DateTime Created { get; set; }
        
        public User CreatedBy { get; set; } 
        
        public Guid CreatedById { get; set; }
        
        public TSelf Original { get; set; }

        public Guid OriginalId { get; set; }
        
        public int Version { get; set; }
    }
}