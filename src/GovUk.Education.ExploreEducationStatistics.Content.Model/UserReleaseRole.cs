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
        
        public bool SoftDeleted { get; set; }

        public UserReleaseRole CreateReleaseAmendment(Release amendment)
        {
            var copy = MemberwiseClone() as UserReleaseRole;
            copy.Id = Guid.NewGuid();
            copy.Release = amendment;
            copy.ReleaseId = amendment.Id;
            return copy;
        }
    }
}