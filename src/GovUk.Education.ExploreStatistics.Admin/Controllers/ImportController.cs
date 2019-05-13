using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private IFileStorageService _fileStorageService;
        private readonly ILogger _logger;

        public ImportController(ILogger<FileController> logger,  IImportService importService, IFileStorageService fileStorageService)
        {
            _logger = logger;
            _importService = importService;
            _fileStorageService = fileStorageService;
        }
        
        public ActionResult Import()
        {
            var model = new CreatePublicationModel
            {
                PublicationsToImport = GetAvailableReleases()
            };

            return View(model);
        }
        
        [HttpPost]
        public IActionResult Import(CreatePublicationModel model)
        {
            var publication = GetAvailableReleases().FirstOrDefault(p => p.PublicationId == model.Id);
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
        
        private List<ImportViewModel> GetAvailableReleases()
        {
            var publications = new List<ImportViewModel>();
            var publicationIds = new HashSet<string>();
            var files = _fileStorageService.ListFiles("releases").Where(f => f.EndsWith("csv"));
            
            // Just get the generated Guids that were generated from the upload
            foreach (var file in files)
            {
                var str = file.Split('/');
                var fName = str.Last();
                var uploadId = str.SkipLast(1).Last();
                fName = Path.GetFileNameWithoutExtension(fName).Replace("_", " ");
                
                if (!fName.StartsWith("meta") && !publicationIds.Contains(uploadId))
                {
                    publicationIds.Add(uploadId);
                    {
                        publications.Add(
                            new ImportViewModel
                            {
                                PublicationId = uploadId,
                                PublicationName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fName.ToLower()),
                                ReleaseName = "2018/2019",
                                ReleaseDate = DateTime.Now.ToString("MM/dd/yyyy")
                            }
                        );
                    }
                }
            }
            return publications;
        }
    }
}