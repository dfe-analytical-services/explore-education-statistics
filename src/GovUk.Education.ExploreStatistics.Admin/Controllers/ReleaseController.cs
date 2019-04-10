using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreStatistics.Admin.Models;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreStatistics.Admin.Controllers
{
    public class ReleaseController : Controller
    {
        private INotificationService _notificationService;

        private readonly ILogger _logger;

        private List<PublicationViewModel> _publications;

        public ReleaseController(ILogger<FileController> logger,  INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;  
                
            _publications = new List<PublicationViewModel>
            {
                new PublicationViewModel
                {
                    PublicationId = "cbbd299f-8297-44bc-92ac-558bcf51f8ad",
                    Name = "Pupil absence in schools in England"
                },
                new PublicationViewModel
                {
                    PublicationId = "bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9",
                    Name = "Permanent and fixed period exclusions"
                },
                new PublicationViewModel
                {
                    PublicationId = "a91d9e05-be82-474c-85ae-4913158406d0",
                    Name = "Schools, pupils and their characteristics"
                }
            };
        }
        
        public ActionResult Notify()
        {
            var model = new CreatePublicationModel
            {
                Publications = _publications
            };

            return View(model);
        }
        
        [HttpPost]
        public IActionResult Notify(CreatePublicationModel model)
        {
            var publication = _publications.FirstOrDefault(p => p.PublicationId == model.Id);
            if (publication == null)
            {
                return new NotFoundResult();
            }

            if (_notificationService.SendNotification(publication))
            {
                return RedirectToAction("NotificationSent", "Release", publication);
            }
            
            return new BadRequestResult();
        }

        [Route("{controller}/notify/sent")]
        public IActionResult NotificationSent(PublicationViewModel model)
        {
            return View(model);
        }
    }
}