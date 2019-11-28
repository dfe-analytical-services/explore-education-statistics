using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DataBlockId = System.Guid;
using ContentSectionId = System.Guid;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using PublicationId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Releases once the current Crud releases controller is removed
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IReleaseService _releaseService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPublicationService _publicationService;
        private readonly IImportStatusService _importStatusService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReleasesController(IImportService importService,
            IReleaseService releaseService,
            IFileStorageService fileStorageService,
            IPublicationService publicationService,
            IImportStatusService importStatusService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            UserManager<ApplicationUser> userManager
            )
        {
            _importService = importService;
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _publicationService = publicationService;
            _importStatusService = importStatusService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _userManager = userManager;
        }

        [HttpGet("release/{releaseId}/chart/{filename}")]
        public async Task<ActionResult> GetChartFile(ReleaseId releaseId, string filename)
        {
            return await CheckReleaseExistsStreamAsync(releaseId,
                () => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Chart, filename));
        }

        [HttpGet("release/{releaseId}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(ReleaseId releaseId, string filename)
        {
            return await CheckReleaseExistsStreamAsync(releaseId,
                () => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Data, filename));
        }

        [HttpGet("release/{releaseId}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(ReleaseId releaseId, string filename)
        {
            return await CheckReleaseExistsStreamAsync(releaseId,
                () => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Ancillary, filename));
        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            PublicationId publicationId)
        {
            return await CheckPublicationExistsAsync(publicationId, () =>
            {
                release.PublicationId = publicationId;
                return _releaseService.CreateReleaseAsync(release);
            });
        }
        
        // GET api/release/{releaseId}/data
        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddAncillaryFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await CheckReleaseExistsAsync(releaseId,
                () => _fileStorageService.UploadFilesAsync(releaseId, file, name,
                    ReleaseFileTypes.Ancillary, false));
        }

        // POST api/release/{releaseId}/chart
        [HttpPost("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddChartFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await CheckReleaseExistsAsync(releaseId,
                () => _fileStorageService.UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Chart, false));
        }

        // POST api/release/{releaseId}/data
        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddDataFilesAsync(ReleaseId releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file, IFormFile metaFile)
        {
            return await CheckReleaseExistsAsync(releaseId, async () =>
            {
                var user = await _userManager.GetUserAsync(User);
                
                // upload the files
                return await _fileStorageService.UploadDataFilesAsync(releaseId, file, metaFile, name, false, user.Email)
                    // add message to queue to process these files
                    .OnSuccess(() => _importService.Import(file.FileName, releaseId));
            });
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ReleaseViewModel> GetReleaseAsync(ReleaseId releaseId)
        {
            return await _releaseService.GetReleaseForIdAsync(releaseId);
        }

        [HttpGet("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            return Ok(await _releaseService.GetReleaseSummaryAsync(releaseId));
        }

        [HttpPut("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateReleaseSummaryAsync(UpdateReleaseSummaryRequest request,
            ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(releaseId, () =>
            {
                return _releaseService.EditReleaseSummaryAsync(releaseId, request);
            });
        }

        // GET api/publications/{publicationId}/releases
        [HttpGet("publications/{publicationId}/releases")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetReleaseForPublicationAsync(
            [Required] PublicationId publicationId)
        {
            return Ok(await _releaseService.GetReleasesForPublicationAsync(publicationId));
        }
        
        // GET api/releases/draft
        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetDraftReleasesAsync()
        {
            return Ok(await _releaseService.GetReleasesForReleaseStatusesAsync(ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview));
        }
        
        // GET api/releases/scheduled
        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return Ok(await _releaseService.GetReleasesForReleaseStatusesAsync(ReleaseStatus.Approved));
        }
        
        [HttpGet("release/{releaseId}/data/{fileName}/import/status")]
        public async Task<ActionResult<ImportStatus>> GetDataUploadStatus(ReleaseId releaseId, string fileName)
        {
            return Ok(await _importStatusService.GetImportStatus(releaseId.ToString(), fileName));
        }

        [HttpDelete("release/{releaseId}/data/{fileName}/{subjectTitle}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteDataFiles(ReleaseId releaseId, string fileName, string subjectTitle)
        {
            return await CheckReleaseExistsAsync(releaseId, async () =>
                {
                    await _tableStorageService.DeleteEntityAsync("imports",
                        new DatafileImport(releaseId.ToString(), fileName, 0,0, null));
                    await _subjectService.DeleteAsync(releaseId, subjectTitle);
                    return await _fileStorageService.DeleteDataFileAsync(releaseId, fileName);
                });
        }

        // DELETE api/release/{releaseId}/ancillary/{fileName}
        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteAncillaryFile(
            ReleaseId releaseId, string fileName)
        {
            return await CheckReleaseExistsAsync(releaseId,
                () => _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Ancillary, fileName));
        }

        // DELETE api/release/{releaseId}/chart/{fileName}
        [HttpDelete("release/{releaseId}/chart/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteChartFile(
            ReleaseId releaseId, string fileName)
        {
            return await CheckReleaseExistsAsync(releaseId,
                () => _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName));
        }

        [HttpPut("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            UpdateReleaseStatusRequest updateRequest, ReleaseId releaseId)
        {
            return await CheckReleaseExistsAsync(
                releaseId, 
                () => _releaseService.UpdateReleaseStatusAsync(releaseId, updateRequest.ReleaseStatus, updateRequest.InternalReleaseNote)
            );
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
                return ValidationProblem(new ValidationProblemDetails(ModelState));
            }

            return Ok(result.Right);
        }

        private async Task<ActionResult> CheckReleaseExistsStreamAsync(ReleaseId releaseId,
            Func<Task<Either<ValidationResult, FileStreamResult>>> andThen)
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
                return ValidationProblem(new ValidationProblemDetails(ModelState));
            }

            return result.Right;
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
                return ValidationProblem(new ValidationProblemDetails(ModelState));
            }

            return Ok(result.Right);
        }
    }
}