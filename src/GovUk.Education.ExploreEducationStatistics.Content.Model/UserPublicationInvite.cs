#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserPublicationInvite : ICreatedTimestamp<DateTime>
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

        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }
    }
}
