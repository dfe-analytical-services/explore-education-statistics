#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ManageAccessPageViewModel
    {
        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; }

        public List<ManageAccessPageReleaseViewModel> Releases { get; set; }

    }
}
