using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers
{
    [Area("Tools")]
    [ApiExplorerSettings(IgnoreApi=true)]
    [Authorize]
    public class ContentCacheController : Controller
    {
        public IActionResult RebuildContentTrees()
        {
            return Ok();
        }
        
        public IActionResult RebuildReleases()
        {
            return Ok();
        }
        
        public IActionResult RebuildMethodology()
        {
            return Ok();
        }
        
        public IActionResult Clean()
        {
            return Ok();
        }
    }
}