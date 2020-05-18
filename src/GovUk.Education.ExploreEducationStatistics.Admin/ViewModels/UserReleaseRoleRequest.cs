using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserReleaseRoleRequest
    {
        public Guid ReleaseId { get; set; }
        public ReleaseRole ReleaseRole { get; set; }
    }
}