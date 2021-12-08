#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ContributorInviteRequest
    {
        public string Email { get; set; }

        public List<Guid> ReleaseIds { get; set; }
    }
}
