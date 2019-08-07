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
        private readonly ApplicationDbContext _context;
        private readonly IPublishingService _publishingService;

        public PublishingController(ApplicationDbContext context, IPublishingService publishingService)
        {
            _context = context;
            _publishingService = publishingService;
        }

        public ActionResult PublishReleaseData()
        {
            ViewData["ReleaseId"] = new SelectList(_context.Releases.OrderBy(r => r.Title), "Id", "Title");
            return View();
        }

        [HttpPost]
        public IActionResult PublishReleaseData(Guid releaseId)
        {
            _publishingService.PublishReleaseData(releaseId);
            return RedirectToAction("ReleaseDataPublished", "Publishing", new {releaseId});
        }

        [Route("{controller}/publish/complete")]
        public IActionResult ReleaseDataPublished(Guid releaseId)
        {
            var release = _context.Releases.FirstOrDefault(r => r.Id.Equals(releaseId));
            return View(release);
        }
    }
}