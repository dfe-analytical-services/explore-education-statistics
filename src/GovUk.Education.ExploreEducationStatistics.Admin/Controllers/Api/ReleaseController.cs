using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReleaseId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Releases once the current Crud releases controller is removed
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportService _importService;
        private readonly IPublicationService _publicationService;

        public ReleasesController(IReleaseService releaseService, IFileStorageService fileStorageService,
            IImportService importService, IPublicationService publicationService)
        {
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _importService = importService;
            _publicationService = publicationService;
        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publications/{publicationId}/releases")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            PublicationId publicationId)
        {
            return await CheckPublicationExistsAsync(publicationId, async () =>
            {
                release.PublicationId = publicationId;
                return await _releaseService.CreateReleaseAsync(release);
            });
        }


        // GET api/release/{releaseId}/data
        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetDataFilesAsync(ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => Ok(await _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Data)));
        }

        // GET api/release/{releaseId}/ancillary
        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => Ok(await _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary)));
        }

        // GET api/release/{releaseId}/chart
        [HttpGet("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetChartFilesAsync(ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => Ok(await _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Chart)));
        }

        // POST api/release/{releaseId}/ancillary
        [HttpPost("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddAncillaryFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => await _fileStorageService.UploadFilesAsync(releaseId, file, name,
                    ReleaseFileTypes.Ancillary));
        }

        // POST api/release/{releaseId}/chart
        [HttpPost("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddChartFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => await _fileStorageService.UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Chart));
        }

        // POST api/release/{releaseId}/data
        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddDataFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file, IFormFile metaFile)
        {
            return await CheckReleaseExistsAsync(releaseId, async () =>
            {
                // upload the files
                var result = await _fileStorageService.UploadDataFilesAsync(releaseId, file, metaFile, name)
                    // add message to queue to process these files
                    .OnSuccess(() => _importService.Import(file.FileName, releaseId));
                return result;
            });
        }

        [HttpGet("releases/{releaseId}")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ReleaseViewModel> GetReleaseAsync(ReleaseId releaseId)
        {
            return await _releaseService.GetReleaseForIdAsync(releaseId);
        }

        [HttpGet("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<EditReleaseSummaryViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            return  Ok(await _releaseService.GetReleaseSummaryAsync(releaseId));
        }

        [HttpPut("releases/{releaseId}/summary")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public async Task<ActionResult<ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseSummaryViewModel model, ReleaseId releaseId)
        {
            return  await CheckReleaseExistsAsync(releaseId, async () =>
            {
                model.Id = releaseId;
                return await _releaseService.EditReleaseSummaryAsync(model);
            });
        }

        // GET api/publications/{publicationId}/releases
        [HttpGet("publications/{publicationId}/releases")]
        [AllowAnonymous] // TODO We will need to do Authorisation checks when we know what the permissions model is.
        public async Task<ActionResult<List<ReleaseViewModel>>> GetReleaseForPublicationAsync(
            [Required] PublicationId publicationId)
        {
            return Ok(await _releaseService.GetReleasesForPublicationAsync(publicationId));
        }

        [HttpDelete("release/{releaseId}/data/{fileName}")]
        [AllowAnonymous] // TODO We will need to do Authorisation checks when we know what the permissions model is.
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteDataFiles(ReleaseId releaseId, string fileName)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => await _fileStorageService.DeleteDataFileAsync(releaseId, fileName));
        }

        // DELETE api/release/{releaseId}/ancillary/{fileName}
        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        [AllowAnonymous] // TODO We will need to do Authorisation checks when we know what the permissions model is.
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteAncillaryFile(
            ReleaseId releaseId, string fileName)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => await _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Ancillary, fileName));
        }

        // DELETE api/release/{releaseId}/chart/{fileName}
        [HttpDelete("release/{releaseId}/chart/{fileName}")]
        [AllowAnonymous] // TODO We will need to do Authorisation checks when we know what the permissions model is.
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteChartFile(
            ReleaseId releaseId, string fileName)
        {
            return await CheckReleaseExistsAsync(releaseId,
                async () => await _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName));
        }

        private async Task<ActionResult> CheckReleaseExistsAsync(ReleaseId releaseId, Func<Task<ActionResult>> andThen)
        {
            var release = await _releaseService.GetAsync(releaseId);
            if (release == null)
            {
                return NotFound();
            }

            return await andThen.Invoke();
        }

        private async Task<ActionResult> CheckReleaseExistsAsync<T>(ReleaseId releaseId,
            Func<Task<Either<ValidationResult, T>>> andThen)
        {
            var release = await _releaseService.GetAsync(releaseId);
            if (release == null)
            {
                return NotFound();
            }

            var result = await andThen.Invoke();
            if (result.IsLeft)
            {
                ValidationUtils.AddErrors(ModelState, result.Left);
                return ValidationProblem();
            }

            return Ok(result.Right);
        }

        private async Task<ActionResult> CheckPublicationExistsAsync<T>(PublicationId publicationId,
            Func<Task<Either<ValidationResult, T>>> andThen)
        {
            var publication = await _publicationService.GetAsync(publicationId);
            if (publication == null)
            {
                return NotFound();
            }

            var result = await andThen.Invoke();
            if (result.IsLeft)
            {
                ValidationUtils.AddErrors(ModelState, result.Left);
                return ValidationProblem();
            }

            return Ok(result.Right);
        }
    }
}