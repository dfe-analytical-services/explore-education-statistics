using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyMethodologyViewModel : MethodologyTitleViewModel
    {
        public PermissionsSet Permissions { get; set; }

        public class PermissionsSet
        {
            public bool CanUpdateMethodology { get; set; }
            public bool CanRemoveMethodology { get; set; }
            public bool CanMakeAmendmentOfMethodology { get; set; }
        }
    }
}