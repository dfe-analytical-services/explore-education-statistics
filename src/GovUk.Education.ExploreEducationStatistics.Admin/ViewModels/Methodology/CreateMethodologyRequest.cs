using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class CreateMethodologyRequest
    {
        [Required] public string Title { get; set; }
    }
}