#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserPublicationInvite
    {
        [Key] 
        [Required]
        public Guid Id { get; set; }

        [Required] public string Email { get; set; } = null!;

        [Required] public Publication Publication { get; set; } = null!;

        [Required]
        public Guid PublicationId { get; set; }
        
        [Required]
        public PublicationRole Role { get; set; }
        
        public bool Accepted { get; set; }

        public bool EmailSent { get; set; }

        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }
        
        public bool SoftDeleted { get; set; }
    }
}
