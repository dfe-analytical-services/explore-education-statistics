using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreStatistics.Admin.Controllers
{
    public class GuidanceController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}