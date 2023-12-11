using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PublicationsController : ControllerBase
{
    // GET: api/v1/publications
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> ListPublications(
        [FromQuery] PublicationsListRequest request)
    {
        return new string[] { "value1", "value2" };
    }
}
