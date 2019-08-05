using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Tools.Controllers
{
    [Area("Tools")]
    [ApiExplorerSettings(IgnoreApi=true)]
    [Authorize]
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public FileController(ApplicationDbContext context,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public IActionResult List()
        {
            var releases = _context.Releases
                .Include(r => r.Publication)
                .ToDictionary(r => r, r => _fileStorageService.ListFiles(r.Publication.Slug, r.Slug));

            return View(releases);
        }
    }
}