using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MyReleaseViewModel : ReleaseViewModel
    {
        public PermissionsSet Permissions { get; set; }

        public class PermissionsSet
        {
            public bool CanAddPrereleaseUsers { get; set; }

            public bool CanUpdateRelease { get; set; }

            public bool CanDeleteRelease { get; set; }

            public bool CanMakeAmendmentOfRelease { get; set; }
        }
    }
}
