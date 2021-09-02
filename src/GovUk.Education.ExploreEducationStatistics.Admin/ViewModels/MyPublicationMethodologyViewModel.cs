#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyPublicationMethodologyViewModel
    {
        public bool Owner { get; set; }

        public MyMethodologyViewModel Methodology { get; set; } = null!;

        public PermissionsSet Permissions { get; set; } = new PermissionsSet();

        public class PermissionsSet
        {
            public bool CanDropMethodology { get; set; }
        }
    }
}
