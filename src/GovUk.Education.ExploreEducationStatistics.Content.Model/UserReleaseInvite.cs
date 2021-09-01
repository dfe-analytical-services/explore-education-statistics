#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserReleaseInvite
    {
        [Key] 
        [Required]
        public Guid Id { get; set; }

        [Required] public string Email { get; set; } = null!;

        [Required] public Release Release { get; set; } = null!;

        [Required]
        public Guid ReleaseId { get; set; }
        
        [Required]
        public ReleaseRole Role { get; set; }
        
        public bool Accepted { get; set; }

        public bool EmailSent { get; set; }

        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }
        
        public bool SoftDeleted { get; set; }
    }
}
