using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Releases once the current Crud releases controller is removed
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportService _importService;

        public ReleasesController(IReleaseService releaseService, IFileStorageService fileStorageService, IImportService importService)
        {
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _importService = importService;

        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publication/{publicationId}/releases")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;
            return await _releaseService.CreateReleaseAsync(release);
        }
        
        
        // POST api/release/{releaseId}/data-files
        [HttpGet("release/{releaseId}/data-files")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetDataFiles(Guid releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            if (release == null)
            {
                return NotFound();
            }

            var files = _fileStorageService.ListFiles(release.Id);
            
            return Ok(files);
        }

        // POST api/release/{releaseId}/data-files
        [HttpPost("release/{releaseId}/data-files")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> AddDataFiles(Guid releaseId, string name, IFormFile file, IFormFile metaFile)
        {
            var release = await _releaseService.GetAsync(releaseId);
            if (release == null)
            {
                return NotFound();
            }
            
            // upload the files
            await _fileStorageService.UploadFilesAsync(release.Id, file, metaFile, name);
            
            // add message to queue to process these files
            _importService.Import(file.FileName, release.Id);

            return Ok();
        }
    }
}