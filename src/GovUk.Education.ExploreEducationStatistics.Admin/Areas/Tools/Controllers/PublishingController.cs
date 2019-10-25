using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers
{
    [Area("Tools")]
    [ApiExplorerSettings(IgnoreApi=true)]
    [Authorize]
    public class PublishingController : Controller
    {
        private readonly ContentDbContext _context;
        private readonly IPublishingService _publishingService;

        public PublishingController(ContentDbContext context, IPublishingService publishingService)
        {
            _context = context;
            _publishingService = publishingService;
        }

        public ActionResult PublishReleaseData()
        {
            var releases = _context.Releases.ToList().OrderBy(release => release.Title);
            ViewData["ReleaseId"] = new SelectList(releases, "Id", "Title");
            return View();
        }

        [HttpPost]
        public IActionResult PublishReleaseData(Guid releaseId)
        {
            _publishingService.PublishReleaseData(releaseId);
            return RedirectToAction("ReleaseDataPublished", "Publishing", new {releaseId});
        }

        [Route("[controller]/publish/complete")]
        public IActionResult ReleaseDataPublished(Guid releaseId)
        {
            var release = _context.Releases.FirstOrDefault(r => r.Id.Equals(releaseId));
            return View(release);
        }
    }
}