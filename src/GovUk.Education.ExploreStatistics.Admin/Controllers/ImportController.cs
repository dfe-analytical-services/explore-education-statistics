using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreStatistics.Admin.Models;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreStatistics.Admin.Controllers
{
    public class ImportController : Controller
    {
        private IImportService _importService;

        private readonly ILogger _logger;

        private List<ImportViewModel> _publications;

        public ImportController(ILogger<FileController> logger,  IImportService importService)
        {
            _logger = logger;
            _importService = importService;  
                
            _publications = new List<ImportViewModel>
            {
                new ImportViewModel
                {
                    PublicationId = "ab4f45fa-20ac-44ac-894f-5513f6b6232d",
                    PublicationName = "Pupil absence in schools in England",
                    ReleaseName = "2018/2019",
                    ReleaseDate = "2018-12-02"
                },
                new ImportViewModel
                {
                    PublicationId = "ab4f45fa-20ac-44ac-894f-6613f6b6232d",
                    PublicationName = "Some dummy",
                    ReleaseName = "2018/2019",
                    ReleaseDate = "2018-12-02"
                }
            };
        }
        
        public ActionResult Import()
        {
            var model = new CreatePublicationModel
            {
                PublicationsToImport = _publications
            };

            return View(model);
        }
        
        [HttpPost]
        public IActionResult Import(CreatePublicationModel model)
        {
            var publication = _publications.FirstOrDefault(p => p.PublicationId == model.Id);
            if (publication == null)
            {
                return new NotFoundResult();
            }

            if (_importService.SendImportNotification(publication))
            {
                return RedirectToAction("ImportNotificationSent", "Import", publication);
            }
            
            return new BadRequestResult();
        }

        [Route("{controller}/import/sent")]
        public IActionResult ImportNotificationSent(ImportViewModel model)
        {
            return View(model);
        }
    }
}