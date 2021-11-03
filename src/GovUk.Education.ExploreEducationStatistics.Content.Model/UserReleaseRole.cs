#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserReleaseRole
    {
        public Guid Id { get; set; }

        public User User { get; set; } = null!;
        
        public Guid UserId { get; set; }

        public Release Release { get; set; } = null!;
        
        public Guid ReleaseId { get; set; }
        
        public ReleaseRole Role { get; set; }

        public Guid? CreatedById { get; set; }

        public User? CreatedBy { get; set; }

        public DateTime? Created { get; set; }

        public Guid? DeletedById { get; set; }

        public User? DeletedBy { get; set; }

        public DateTime? Deleted { get; set; }
        
        public bool SoftDeleted { get; set; }



        public UserReleaseRole CopyForAmendment(Release amendment)
        {
            var copy = MemberwiseClone() as UserReleaseRole;
            copy.Id = Guid.NewGuid();
            copy.Release = amendment;
            copy.ReleaseId = amendment.Id;
            return copy;
        }
    }
}
