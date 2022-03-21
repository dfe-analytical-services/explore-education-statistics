using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly IReleaseApprovalService _releaseApprovalService;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataImportService _dataImportService;

        public ReleasesController(
            IReleaseService releaseService,
            IReleaseApprovalService releaseApprovalService,
            IReleaseDataFileService releaseDataFileService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IReleaseChecklistService releaseChecklistService,
            UserManager<ApplicationUser> userManager,
            IDataImportService dataImportService)
        {
            _releaseService = releaseService;
            _releaseDataFileService = releaseDataFileService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _releaseChecklistService = releaseChecklistService;
            _userManager = userManager;
            _dataImportService = dataImportService;
            _releaseApprovalService = releaseApprovalService;
        }

        [HttpPost("publications/{publicationId}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateRelease(ReleaseCreateViewModel release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateRelease(release)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> DeleteRelease(Guid releaseId)
        {
            return await _releaseService
                .DeleteRelease(releaseId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId}/amendment")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAmendment(Guid releaseId)
        {
            return await _releaseService
                .CreateReleaseAmendment(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileId}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataFileInfo>> GetDataFileInfo(Guid releaseId, Guid fileId)
        {
            return await _releaseDataFileService
                .GetInfo(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<DataFileInfo>>> GetDataFileInfo(Guid releaseId)
        {
            return await _releaseDataFileService
                .ListAll(releaseId)
                .HandleFailuresOrOk();
        }


        [HttpPost("release/{releaseId}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataFilesAsync(Guid releaseId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "title")] string subjectName,
            IFormFile file,
            IFormFile metaFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseDataFileService
                .Upload(releaseId: releaseId,
                    dataFormFile: file,
                    metaFormFile: metaFile,
                    userName: user.Email,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/zip-data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataZipFileAsync(Guid releaseId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "title")] string subjectName,
            IFormFile zipFile)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _releaseDataFileService
                .UploadAsZip(releaseId: releaseId,
                    zipFormFile: zipFile,
                    userName: user.Email,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(Guid releaseId)
        {
            return await _releaseService
                .GetRelease(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/status")]
        public async Task<ActionResult<List<ReleaseStatusViewModel>>> GetReleaseStatuses(Guid releaseId)
        {
            return await _releaseApprovalService
                .GetReleaseStatuses(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseId)
        {
            return await _releaseService
                .GetReleasePublicationStatus(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId}")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateRelease(ReleaseUpdateViewModel request,
            Guid releaseId)
        {
            return await _releaseService
                .UpdateRelease(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("releases/{releaseId}/status")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseStatus(ReleaseStatusCreateViewModel request,
            Guid releaseId)
        {
            return await _releaseApprovalService
                .CreateReleaseStatus(releaseId, request)
                .OnSuccess(_ => _releaseService.GetRelease(releaseId))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/releases/template")]
        public async Task<ActionResult<TitleAndIdViewModel>> GetTemplateRelease(
            [Required] Guid publicationId)
        {
            return await _releaseService
                .GetLatestPublishedRelease(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetDraftReleasesAsync()
        {
            return await _releaseService
                .GetMyReleasesForReleaseStatusesAsync(ReleaseApprovalStatus.Draft, ReleaseApprovalStatus.HigherLevelReview)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<MyReleaseViewModel>>> GetScheduledReleasesAsync()
        {
            return await _releaseService
                .GetMyScheduledReleasesAsync()
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileId}/import/status")]
        public Task<ActionResult<DataImportViewModel>> GetDataUploadStatus(Guid releaseId, Guid fileId)
        {
            return _releaseService
                .GetDataFileImportStatus(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/delete-plan")]
        public async Task<ActionResult<DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseId)
        {
            return await _releaseService
                .GetDeleteReleasePlan(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/data/{fileId}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId}/data/{fileId}")]
        public async Task<ActionResult> DeleteDataFiles(Guid releaseId, Guid fileId)
        {
            return await _releaseService
                .RemoveDataFiles(releaseId, fileId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId}/data/{fileId}/import/cancel")]
        public async Task<ActionResult> CancelFileImport(Guid releaseId, Guid fileId)
        {
            return await _dataImportService
                .CancelImport(releaseId, fileId)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpGet("releases/{releaseId}/stage-status")]
        public async Task<ActionResult<ReleasePublishingStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _releasePublishingStatusService
                .GetReleaseStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId}/checklist")]
        public async Task<ActionResult<ReleaseChecklistViewModel>> GetChecklist(Guid releaseId)
        {
            return await _releaseChecklistService
                .GetChecklist(releaseId)
                .HandleFailuresOrOk();
        }
    }
}
