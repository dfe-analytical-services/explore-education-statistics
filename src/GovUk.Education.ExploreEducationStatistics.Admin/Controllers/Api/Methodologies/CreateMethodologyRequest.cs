using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class CreateMethodologyRequest
    {
        [Required] public string Title { get; set; }
    }
}