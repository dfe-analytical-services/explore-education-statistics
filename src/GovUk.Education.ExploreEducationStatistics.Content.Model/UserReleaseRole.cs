using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserReleaseRole
    {
        public Guid Id { get; set; }
        
        public User User { get; set; }
        
        public Guid UserId { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }
        
        public ReleaseRole Role { get; set; }
    }
}