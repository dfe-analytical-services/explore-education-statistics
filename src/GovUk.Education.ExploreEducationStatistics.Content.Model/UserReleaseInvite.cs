using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserReleaseInvite
    {
        [Key] 
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public Release Release { get; set; }

        [Required]
        public Guid ReleaseId { get; set; }
        
        [Required]
        public ReleaseRole Role { get; set; }
    }
}