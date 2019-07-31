using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserDetailsViewModel

    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string[] Permissions { get; set; }
    }
}