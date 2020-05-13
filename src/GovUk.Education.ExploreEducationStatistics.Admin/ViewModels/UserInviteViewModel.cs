using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserInviteViewModel
    {
        public string Email { get; set; }
        
        public bool Accepted { get; set; }
        
        public ReleaseRole Role { get; set; }
        
        public DateTime Created { get; set; }
        
        public string CreatedBy { get; set; }
    }
}