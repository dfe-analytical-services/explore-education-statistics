#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ManageAccessPageReleaseViewModel
    {
        public Guid ReleaseId { get; set; }

        public string ReleaseTitle { get; set; }

        public List<ManageAccessPageContributorViewModel> UserList { get; set; }
    }
}
