using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class MethodologyController : ControllerBase
    {
        public MethodologyController()
        {
        }

        [HttpGet("methodologies/{slug}")]
        public async Task<ActionResult<MethodologyViewModel>> Get(string slug)
        {
            // TODO SOW4 EES-2375 Return methodology from content database
            return new NotFoundResult();
        }
    }
}
