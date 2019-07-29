using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class SigninController : Controller
    {
        // GET
        public IActionResult Index()
        {
            Response.Cookies.Append(
                "DFEUserDetails", 
                User.Identity.Name.Split('#').Last(), 
                new CookieOptions { IsEssential = true });
            
            return Redirect("/admin-dashboard");
        }
    }
}