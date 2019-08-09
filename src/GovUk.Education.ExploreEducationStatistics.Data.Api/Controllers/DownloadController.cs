using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        // TODO This endpoint is deprecated and needs to be removed when all file release file types are supported by the front end not just data.
        [HttpGet("{publication}/{release}/{filename}")]
        public async Task<ActionResult> GetFile(string publication, string release, string filename)
            => await GetFile(publication, release, ReleaseFileTypes.Data, filename);

        [HttpGet("{publication}/{release}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(string publication, string release, string filename)
            => await GetFile(publication, release, ReleaseFileTypes.Data, filename);

        [HttpGet("{publication}/{release}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(string publication, string release, string filename)
            => await GetFile(publication, release, ReleaseFileTypes.Ancillary, filename);

        [HttpGet("{publication}/{release}/chart/{filename}")]
        public async Task<ActionResult> GetChartFile(string publication, string release, string filename)
            => await GetFile(publication, release, ReleaseFileTypes.Chart, filename);

        private async Task<ActionResult> GetFile(string publication, string release, ReleaseFileTypes type,
            string filename)
        {
            if (!_fileStorageService.FileExistsAndIsReleased(publication, release, type, filename))
            {
                return NotFound();
            }

            return await _fileStorageService.StreamFile(publication, release, type, filename);
        }
    }
}