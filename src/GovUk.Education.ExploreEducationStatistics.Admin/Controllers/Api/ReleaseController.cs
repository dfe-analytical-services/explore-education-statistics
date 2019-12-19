using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using DataBlockId = System.Guid;
using ContentSectionId = System.Guid;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using PublicationId = System.Guid;
using ReleaseId = System.Guid;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

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
        private readonly IImportStatusService _importStatusService;
        private readonly IPublishingService _publishingService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly UserManager<ApplicationUser> _userManager;
        // TODO EES-918 - remove in favour of checking for release inside service calls
        private readonly IPersistenceHelper<Release, DataBlockId> _releaseHelper;
        // TODO EES-918 - remove in favour of checking for release inside service calls
        private readonly IPersistenceHelper<Publication, DataBlockId> _publicationHelper;

        public ReleasesController(IImportService importService,
            IReleaseService releaseService,
            IFileStorageService fileStorageService,
            IImportStatusService importStatusService,
            IPublishingService publishingService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            UserManager<ApplicationUser> userManager,
            IPersistenceHelper<Release, DataBlockId> releaseHelper,
            IPersistenceHelper<Publication, DataBlockId> publicationHelper
            )
        {
            _importService = importService;
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _importStatusService = importStatusService;
            _publishingService = publishingService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _userManager = userManager;
            _releaseHelper = releaseHelper;
            _publicationHelper = publicationHelper;
        }

        [HttpGet("release/{releaseId}/chart/{filename}")]
        public async Task<ActionResult> GetChartFile(ReleaseId releaseId, string filename)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Chart, filename))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(ReleaseId releaseId, string filename)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Data, filename))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(ReleaseId releaseId, string filename)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Ancillary, filename))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            PublicationId publicationId)
        {
            return await _publicationHelper
                .CheckEntityExistsActionResult(publicationId)
                .OnSuccess(() =>
                {
                    release.PublicationId = publicationId;
                    return _releaseService.CreateReleaseAsync(release);
                })
                .OnSuccess(Ok)
                .HandleFailures();
        }
        
        // GET api/release/{releaseId}/data
        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetDataFilesAsync(ReleaseId releaseId)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Data))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // GET api/release/{releaseId}/ancillary
        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(ReleaseId releaseId)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // GET api/release/{releaseId}/chart
        [HttpGet("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetChartFilesAsync(ReleaseId releaseId)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Chart))
                .OnSuccess(Ok)
                .HandleFailures();
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
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.UploadFilesAsync(releaseId, file, name,
                    ReleaseFileTypes.Ancillary, false))
                .OnSuccess(Ok)
                .HandleFailures();
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
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Chart, false))
                .OnSuccess(Ok)
                .HandleFailures();
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
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                // upload the files
                .OnSuccess(async _ =>
                {
                    var user = await _userManager.GetUserAsync(User);

                    return await _fileStorageService.UploadDataFilesAsync(releaseId, file, metaFile, name, false,
                            user.Email);
                })
                // add message to queue to process these files
                .OnSuccessDo(() => _importService.Import(file.FileName, releaseId))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ReleaseViewModel> GetReleaseAsync(ReleaseId releaseId)
        {
            return await _releaseService.GetReleaseForIdAsync(releaseId);
        }

        [HttpGet("releases/{releaseId}/summary")]
        public Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryAsync(ReleaseId releaseId)
        {
            return HandleErrorsAsync(() => _releaseService.GetReleaseSummaryAsync(releaseId), Ok);
        }

        [HttpPut("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateReleaseSummaryAsync(UpdateReleaseSummaryRequest request,
            ReleaseId releaseId)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _releaseService.EditReleaseSummaryAsync(releaseId, request))
                .OnSuccess(Ok)
                .HandleFailures();
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
            return Ok(await _releaseService.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview));
        }
        
        // GET api/releases/scheduled
        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return Ok(await _releaseService.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved));
        }
        
        [HttpGet("release/{releaseId}/data/{fileName}/import/status")]
        public async Task<ActionResult<ImportStatus>> GetDataUploadStatus(ReleaseId releaseId, string fileName)
        {
            return Ok(await _importStatusService.GetImportStatus(releaseId.ToString(), fileName));
        }

        [HttpDelete("release/{releaseId}/data/{fileName}/{subjectTitle}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteDataFiles(ReleaseId releaseId, string fileName, string subjectTitle)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(async _ =>
                {
                    await _tableStorageService.DeleteEntityAsync("imports",
                        new DatafileImport(releaseId.ToString(), fileName, 0,0, null));
                    await _subjectService.DeleteAsync(releaseId, subjectTitle);
                    return await _fileStorageService.DeleteDataFileAsync(releaseId, fileName);
                })
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // DELETE api/release/{releaseId}/ancillary/{fileName}
        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteAncillaryFile(
            ReleaseId releaseId, string fileName)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ => _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // DELETE api/release/{releaseId}/chart/{fileName}
        [HttpDelete("release/{releaseId}/chart/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteChartFile(
            ReleaseId releaseId, string fileName)
        {
            return await _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_ =>  _fileStorageService.DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName))
                .OnSuccess(Ok)
                .HandleFailures();
        }

        // TODO EES-927 This can be removed once the queue release service call is made when a Release is approved in the UI
        [HttpGet("releases/{releaseId}/queue")]
        public async Task<ActionResult<ValidateReleaseMessage>> QueueReleaseAsync(ReleaseId releaseId)
        {
            return Ok(await _publishingService.QueueReleaseAsync(releaseId));
        }
        
        [HttpPut("releases/{releaseId}/status")]
        public Task<ActionResult<ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            UpdateReleaseStatusRequest updateRequest, ReleaseId releaseId)
        {
            return HandleErrorsAsync(
                () => _releaseService.UpdateReleaseStatusAsync(
                    releaseId, 
                    updateRequest.ReleaseStatus, 
                    updateRequest.InternalReleaseNote),
                Ok);
        }
    }
}