#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleaseContributorViewModel
    {
        public Guid UserId { get; set; }

        public string UserFullName { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public Guid? ReleaseRoleId { get; set; }
    }
}
