using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Authorize]
    public class FileUploadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportService _importService;

        public FileUploadController(ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IImportService importService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _importService = _importService;
        }

        public IActionResult Upload()
        {
            ViewData["ReleaseId"] = new SelectList(_context.Releases.OrderBy(r => r.Title), "Id", "Title");
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(200 * 1024 * 1024)]
        public async Task<IActionResult> Post(Guid releaseId, string name, IFormFile file)
        {
            var release = _context.Releases
                .Where(r => r.Id.Equals(releaseId))
                .Include(r => r.Publication)
                .FirstOrDefault();

            var path = Path.GetTempFileName();
            await UploadFileAsync(release.Publication.Slug, release.Slug, file, path, name);

            return RedirectToAction("List", "File");
        }

        private async Task UploadFileAsync(string publication, string release, IFormFile file, string path, string name)
        {
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            await _fileStorageService.UploadFileAsync(publication, release, file.FileName, path, name);
        }
    }
}