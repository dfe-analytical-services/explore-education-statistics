using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserDetailsPermissionsViewModel

    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string[] Permissions { get; set; }
    }
}