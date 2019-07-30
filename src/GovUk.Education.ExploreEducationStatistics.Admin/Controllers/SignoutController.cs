using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class SignoutController : Controller
    {
        // GET
        public IActionResult Index()
        {
            Response.Cookies.Delete("DFEUserDetails");
            Response.Cookies.Delete(".AspNetCore.AzureADCookie");
            
            return Redirect("/signed-out");
        }
    }
}