using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreStatistics.Admin.Models;
using GovUk.Education.ExploreStatistics.Admin.Services;
using GovUk.Education.ExploreStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreStatistics.Admin.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private IFileStorageService _fileStorageService;
        private readonly ILogger _logger;

        public FileController(ILogger<FileController> logger, IFileStorageService fileStorageService)
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> List()
        {
            var files = _fileStorageService.ListFiles("releases");
            
            var model = new List<FileViewModel>();

            foreach (var file in files)
            {
                model.Add(new FileViewModel
                {
                    Path = file,
                    FileName = file.Split('/').Last(),
                    UploadId = file.Split('/').SkipLast(1).Last()
                });
            }
            return View(model);
        }

        public async Task<IActionResult> Upload()
        {
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(200 * 1024 * 1024)]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            _logger.LogInformation("Received files for upload");

            long size = files.Sum(f => f.Length);

            var filePath = Path.GetTempFileName();

            var releaseId = Guid.NewGuid();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                await _fileStorageService.UploadFileAsync(filePath, file.FileName, releaseId);
            }

            return RedirectToAction("List");
        }
    }
}