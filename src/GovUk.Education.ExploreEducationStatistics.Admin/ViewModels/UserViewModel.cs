using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public string Role { get; set; }
        
        public List<UserReleaseRoleViewModel> UserReleaseRoles { get; set; }
    }

    public class UserReleaseRoleViewModel
    {
        public Guid Id { get; set; }
        public IdTitlePair Publication { get; set; }
        public IdTitlePair Release { get; set; }
        public EnumExtensions.EnumValue ReleaseRole { get; set; }
    }
}