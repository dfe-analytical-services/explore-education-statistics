using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PublicationsController : ControllerBase
{
    // GET: api/v1/publications
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> Get()
    {
        return new string[] { "value1", "value2" };
    }
}
