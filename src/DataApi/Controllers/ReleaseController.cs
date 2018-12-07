using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        [HttpGet("{releaseId}")]
        public ActionResult<string> Get(int releaseId)
        {
            return "value";
        }
    }
}