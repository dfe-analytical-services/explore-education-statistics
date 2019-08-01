using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class SigninController : ControllerBase
    {
        // GET
        public IActionResult Index()
        {
            return Redirect("/");
        }
    }
}