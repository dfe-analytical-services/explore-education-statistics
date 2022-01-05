#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContributorsAndInvitesViewModel
    {
        public List<ContributorViewModel> Contributors { get; set; }

        public List<string> PendingInviteEmails { get; set; }
    }
}
