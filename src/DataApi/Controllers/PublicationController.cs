using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        [HttpGet("{publicationId}")]
        public ActionResult<string> Get(int publicationId)
        {
            return "value";
        }
    }
}