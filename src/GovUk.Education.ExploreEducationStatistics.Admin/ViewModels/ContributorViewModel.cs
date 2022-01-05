#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContributorViewModel
    {
        public Guid? UserId { get; set; }

        public string? UserDisplayName { get; set; }

        public string UserEmail { get; set; } = "";
    }
}
