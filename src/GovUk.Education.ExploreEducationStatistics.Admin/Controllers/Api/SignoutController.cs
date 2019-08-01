using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class SignoutController : ControllerBase
    {
        // GET
        public IActionResult Index()
        {
            // TODO - we'll probably need to do something more robust to tell AD that
            // the user is logging out of this particular service - at the moment hitting
            // sign-in would immediately log them back in as AD will recognise that this
            // user has already requested a cookie before and will reissue it without
            // enforcing another login
            Response.Cookies.Delete(".AspNetCore.AzureADCookie");
            return Redirect("/signed-out");
        }
    }
}