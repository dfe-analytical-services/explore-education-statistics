using System;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
            return Redirect("/admin-dashboard");
        }
    }
}