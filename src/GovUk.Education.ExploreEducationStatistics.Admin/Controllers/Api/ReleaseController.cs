using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

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
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReleasesController(IImportService importService,
            IReleaseService releaseService,
            IFileStorageService fileStorageService,
            IReleaseStatusService releaseStatusService,
            UserManager<ApplicationUser> userManager
            )
        {
            _importService = importService;
            _releaseService = releaseService;
            _fileStorageService = fileStorageService;
            _releaseStatusService = releaseStatusService;
            _userManager = userManager;
        }

        [HttpGet("release/{releaseId}/chart/{filename}")]
        public async Task<ActionResult> GetChartFile(Guid releaseId, string filename)
        {
            return await _fileStorageService
                .StreamFile(releaseId, ReleaseFileTypes.Chart, filename)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/data/{filename}")]
        public async Task<ActionResult> GetDataFile(Guid releaseId, string filename)
        {
            return await _fileStorageService
                .StreamFile(releaseId, ReleaseFileTypes.Data, filename)
                .HandleFailures();
        }

        [HttpGet("release/{releaseId}/ancillary/{filename}")]
        public async Task<ActionResult> GetAncillaryFile(Guid releaseId, string filename)
        {
            return await _fileStorageService
                .StreamFile(releaseId, ReleaseFileTypes.Ancillary, filename)
                .HandleFailures();
        }

        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateReleaseAsync(release)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> DeleteReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .DeleteReleaseAsync(releaseId)
                .HandleFailuresOr(_ => NoContent());
        }
        
        [HttpPost("release/{releaseId}/amendment")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId)
        {
            return await _releaseService
                .CreateReleaseAmendmentAsync(releaseId)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetDataFilesAsync(Guid releaseId)
        {
            return await _fileStorageService
                .ListFilesAsync(releaseId, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/ancillary")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFilesAsync(Guid releaseId)
        {
            return await _fileStorageService
                .ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/chart")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetChartFilesAsync(Guid releaseId)
        {
            return await _fileStorageService
                .ListFilesAsync(releaseId, ReleaseFileTypes.Chart)
                .HandleFailuresOrOk();
        }

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
                .HandleFailuresOrOk();
        }

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
                .UploadFilesAsync(releaseId, file, name, ReleaseFileTypes.Chart, true)
                .HandleFailuresOrOk();
        }

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
                .OnSuccessDo(() => _importService.Import(file.FileName, releaseId, file))
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> GetReleaseAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseForIdAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> GetReleaseSummaryAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleaseSummaryAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(Guid releaseId)
        {
            return await _releaseService
                .GetReleasePublicationStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}/summary")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateReleaseSummaryAsync(UpdateReleaseSummaryRequest request,
            Guid releaseId)
        {
            return await _releaseService
                .EditReleaseSummaryAsync(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/releases/template")]
        public async Task<ActionResult<TitleAndIdViewModel?>> GetTemplateReleaseAsync(
            [Required] Guid publicationId)
        {
            return await _releaseService
                .GetLatestReleaseAsync(publicationId)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetDraftReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Draft, ReleaseStatus.HigherLevelReview)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("release/{releaseId}/data/{fileName}/import/status")]
        public Task<ActionResult<ImportStatus>> GetDataUploadStatus(Guid releaseId, string fileName)
        {
            return _releaseService
                .GetDataFileImportStatus(releaseId, fileName)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileName}/{subjectTitle}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, string fileName, string subjectTitle)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseId, fileName, subjectTitle)
                .HandleFailuresOrOk();
        }
        
        [HttpDelete("release/{releaseId}/data/{fileName}/{subjectTitle}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteDataFiles(Guid releaseId, string fileName, string subjectTitle)
        {
            return await _releaseService
                .DeleteDataFilesAsync(releaseId, fileName, subjectTitle)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/ancillary/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteAncillaryFile(
            Guid releaseId, string fileName)
        {
            return await _fileStorageService
                .DeleteFileAsync(releaseId, ReleaseFileTypes.Ancillary, fileName)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/chart/{fileName}")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> DeleteChartFile(
            Guid releaseId, string fileName)
        {
            return await _fileStorageService
                .DeleteFileAsync(releaseId, ReleaseFileTypes.Chart, fileName)
                .HandleFailuresOrOk();
        }
        
        [HttpGet("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _releaseStatusService
                .GetReleaseStatusesAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(
            UpdateReleaseStatusRequest updateRequest, Guid releaseId)
        {
            return await _releaseService
                .UpdateReleaseStatusAsync(releaseId, updateRequest.ReleaseStatus, updateRequest.InternalReleaseNote)
                .HandleFailuresOrOk();
        }
    }
}