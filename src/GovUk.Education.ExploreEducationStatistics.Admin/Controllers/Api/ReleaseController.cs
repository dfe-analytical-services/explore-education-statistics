using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;

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
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly UserManager<ApplicationUser> _userManager;
        // TODO EES-918 - remove in favour of checking for release inside service calls
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public ReleasesController(IImportService importService,
            IReleaseService releaseService,
            IFileStorageService fileStorageService,
            IImportStatusService importStatusService,
            IReleaseStatusService releaseStatusService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            UserManager<ApplicationUser> userManager,
            IPersistenceHelper<ContentDbContext> persistenceHelper
            )
        {
            _importService = importService;
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _importStatusService = importStatusService;
            _releaseStatusService = releaseStatusService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _userManager = userManager;
            _persistenceHelper = persistenceHelper;
        }

        [HttpGet("release/{releaseId}/chart/{filename}")]
        public async Task<ActionResult> GetChartFile(Guid releaseId, string filename)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Chart, filename))
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(Guid releaseId, string filename)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Data, filename))
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(Guid releaseId, string filename)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.StreamFile(releaseId, ReleaseFileTypes.Ancillary, filename))
                .HandleFailures();
        }

        // POST api/publication/{publicationId}/releases
        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateReleaseAsync(release)
                .HandleFailuresOr(Ok);
        }
        
        // GET api/release/{releaseId}/data
        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetDataFilesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Data))
                .HandleFailuresOr(Ok);
        }

        // GET api/release/{releaseId}/ancillary
        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary))
                .HandleFailuresOr(Ok);
        }

        // GET api/release/{releaseId}/chart
        [HttpGet("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetChartFilesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_ => _fileStorageService.ListFilesAsync(releaseId, ReleaseFileTypes.Chart))
                .HandleFailuresOr(Ok);
        }

        // POST api/release/{releaseId}/ancillary
        [HttpPost("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddAncillaryFilesAsync(Guid releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await _fileStorageService
                .UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Ancillary, false)
                .HandleFailuresOr(Ok);
        }

        // POST api/release/{releaseId}/chart
        [HttpPost("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddChartFilesAsync(Guid releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file)
        {
            return await _fileStorageService
                .UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Chart, false)
                .HandleFailuresOr(Ok);
        }

        // POST api/release/{releaseId}/data
        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> AddDataFilesAsync(Guid releaseId,
            [Required] [FromQuery(Name = "name")] string name, IFormFile file, IFormFile metaFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _fileStorageService
                .UploadDataFilesAsync(releaseId, file, metaFile, name, false, user.Email)
                // add message to queue to process these files
                .OnSuccessDo(() => _importService.Import(file.FileName, releaseId))
                .HandleFailuresOr(Ok);
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> GetReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseForIdAsync(releaseId)
                .HandleFailuresOr(Ok);
        }

        [HttpGet("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseSummaryAsync(releaseId)
                .HandleFailuresOr(Ok);
        }

        [HttpPut("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateReleaseSummaryAsync(UpdateReleaseSummaryRequest request,
            Guid releaseId)
        {
            return await _releaseService
                .EditReleaseSummaryAsync(releaseId, request)
                .HandleFailuresOr(Ok);
        }

        // GET api/publications/{publicationId}/releases/template
        [HttpGet("publications/{publicationId}/releases/template")]
        public async Task<ActionResult<TitleAndIdViewModel?>> GetTemplateReleaseAsync(
            [Required] Guid publicationId)
        {
            return await _releaseService
                .GetLatestReleaseAsync(publicationId)
                .HandleFailuresOr(releaseId => new OkObjectResult(releaseId));
        }
        
        // GET api/releases/draft
        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetDraftReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview)
                .HandleFailuresOr(Ok);
        }
        
        // GET api/releases/scheduled
        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<ReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved)
                .HandleFailuresOr(Ok);
        }
        
        [HttpGet("release/{releaseId}/data/{fileName}/import/status")]
        public async Task<ActionResult<ImportStatus>> GetDataUploadStatus(Guid releaseId, string fileName)
        {
            return Ok(await _importStatusService.GetImportStatus(releaseId.ToString(), fileName));
        }

        [HttpDelete("release/{releaseId}/data/{fileName}/{subjectTitle}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteDataFiles(Guid releaseId, string fileName, string subjectTitle)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async _ =>
                {
                    await _tableStorageService.DeleteEntityAsync("imports",
                        new DatafileImport(releaseId.ToString(), fileName, 0,0, null));
                    await _subjectService.DeleteAsync(releaseId, subjectTitle);
                    return await _fileStorageService.DeleteDataFileAsync(releaseId, fileName);
                })
                .HandleFailuresOr(Ok);
        }

        // DELETE api/release/{releaseId}/ancillary/{fileName}
        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteAncillaryFile(
            Guid releaseId, string fileName)
        {
            return await _fileStorageService
                .DeleteFileAsync(releaseId, ReleaseFileTypes.Ancillary, fileName)
                .HandleFailuresOr(Ok);
        }

        // DELETE api/release/{releaseId}/chart/{fileName}
        [HttpDelete("release/{releaseId}/chart/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteChartFile(
            Guid releaseId, string fileName)
        {
            return await _fileStorageService
                .DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName)
                .HandleFailuresOr(Ok);
        }
        
        [HttpGet("releases/{releaseId}/status")]
        public async Task<ActionResult<IEnumerable<ReleaseStatusViewModel>>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return Ok(await _releaseStatusService.GetReleaseStatusesAsync(releaseId));
        }

        [HttpPut("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            UpdateReleaseStatusRequest updateRequest, Guid releaseId)
        {
            return await _releaseService
                .UpdateReleaseStatusAsync(releaseId, updateRequest.ReleaseStatus, updateRequest.InternalReleaseNote)
                .HandleFailuresOr(Ok);
        }
    }
}