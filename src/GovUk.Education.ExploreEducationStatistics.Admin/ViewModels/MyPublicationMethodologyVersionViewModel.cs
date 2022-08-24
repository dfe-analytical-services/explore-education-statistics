#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyPublicationMethodologyVersionViewModel
    {
        public bool Owner { get; set; }

        public MyMethodologyVersionViewModel Methodology { get; set; } = null!;

        public PermissionsSet Permissions { get; set; } = new PermissionsSet();

        public record PermissionsSet
        {
            public bool CanDropMethodology { get; set; }
        }
    }
}
