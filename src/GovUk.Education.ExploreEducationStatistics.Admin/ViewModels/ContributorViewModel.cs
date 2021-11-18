#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContributorViewModel
    {
        public Guid UserId { get; set; }

        public string UserFullName { get; set; } = null!;

        public string UserEmail { get; set; } = null!;

        public Guid? ReleaseRoleId { get; set; }
    }
}
