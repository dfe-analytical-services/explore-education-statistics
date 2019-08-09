using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private const string ContainerName = "downloads";
        private readonly IFileStorageService _fileStorageService;

        public DownloadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{publication}/{release}/{fileName}")]
        public async Task<ActionResult> GetFile(string publication, string release, string fileName)
        {
            var blobName = $"{publication}/{release}/{fileName}";
            if (!await _fileStorageService.FileExistsAndIsReleased(ContainerName, blobName))
            {
                return NotFound();
            }

            return await _fileStorageService.StreamFile(ContainerName, blobName, fileName);
        }
    }
}