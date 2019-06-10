using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public DownloadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{publication}/{release}/{filename}")]
        public async Task<ActionResult> GetFile(string publication, string release, string filename)
        {
            if (!_fileStorageService.FileExistsAndIsReleased(publication, release, filename))
            {
                return NotFound();
            }

            return await _fileStorageService.StreamFile(publication, release, filename);
        }
    }
}