using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyCreateRequest
    {
        [Required] public string Title { get; set; }
    }
}