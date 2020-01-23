using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models
{
    public class UserInvite
    {
        [Key]
        public string Email { get; set; }
        
        public bool Accepted { get; set; }
        
        public IdentityRole Role { get; set; }
        
        public Guid RoleId { get; set; }
        
        public DateTime Created { get; set; }
        
        public string CreatedBy { get; set; }
    }
}