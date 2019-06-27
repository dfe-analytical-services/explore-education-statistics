using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationsService _notificationsService;

        public NotificationsController(ApplicationDbContext context,
            INotificationsService notificationsService)
        {
            _context = context;
            _notificationsService = notificationsService;
        }

        public ActionResult NotifySubscribers()
        {
            ViewData["PublicationId"] = new SelectList(_context.Releases.GroupBy(release => release.Publication)
                .Select(releases => releases.Key).OrderBy(r => r.Title), "Id", "Title");

            return View();
        }

        [HttpPost]
        public IActionResult NotifySubscribers(Guid publicationId)
        {
            if (_notificationsService.NotifySubscribers(publicationId))
            {
                return RedirectToAction("NotificationsSent", "Notifications", new {publicationId});
            }

            return new BadRequestResult();
        }

        [Route("{controller}/notify/sent")]
        public IActionResult NotificationsSent(Guid publicationId)
        {
            var publication = _context.Publications.FirstOrDefault(p => p.Id.Equals(publicationId));
            return View(publication);
        }
    }
}