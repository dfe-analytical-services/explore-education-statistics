#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyPublicationMethodologyViewModel
    {
        public bool Owner { get; set; }

        public MyMethodologyViewModel Methodology { get; set; } = null!;
    }
}
