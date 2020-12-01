using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.ReleaseFileTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

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

        [HttpGet("{publication}/{release}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(string publication, string release, string filename)
        {
            return await GetFile(PublicReleasePath(publication, release, ReleaseFileTypes.Data, filename));
        }

        [HttpGet("{publication}/{release}/ancillary/{fileId}")]
        public async Task<ActionResult> GetAncillaryFile(string publication, string release, string fileId)
        {
            if (Guid.TryParse(fileId, out var idAsGuid))
            {
                return await GetFile(PublicReleasePath(publication, release, Ancillary, idAsGuid));
            }

            return NotFound();
        }

        [HttpGet("{publication}/{release}/chart/{fileId}")]
        public async Task<ActionResult> GetChartFile(string publication, string release, string fileId)
        {
            if (Guid.TryParse(fileId, out var idAsGuid))
            {
                return await GetFile(PublicReleasePath(publication, release, Chart, idAsGuid));
            }

            return NotFound();
        }

        private async Task<ActionResult> GetFile(string path)
        {
            var isReleased = await _fileStorageService.IsBlobReleased(
                containerName: PublicFilesContainerName,
                path: path
            );

            if (!isReleased)
            {
                return NotFound();
            }

            try
            {
                return await _fileStorageService.StreamFile(
                    containerName: PublicFilesContainerName,
                    path: path
                );
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}