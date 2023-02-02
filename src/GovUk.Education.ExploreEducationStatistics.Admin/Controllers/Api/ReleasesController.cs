using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IDataImportService _dataImportService;

        public ReleasesController(
            IReleaseService releaseService,
            IReleaseApprovalService releaseApprovalService,
            IReleaseDataFileService releaseDataFileService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IReleaseChecklistService releaseChecklistService,
            IDataImportService dataImportService)
        {
            _releaseService = releaseService;
            _releaseDataFileService = releaseDataFileService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _releaseChecklistService = releaseChecklistService;
            _dataImportService = dataImportService;
            _releaseApprovalService = releaseApprovalService;
        }

        [HttpPost("publications/{publicationId:guid}/releases")]
        public async Task<ActionResult<ReleaseViewModel>> CreateRelease(ReleaseCreateRequest release,
            Guid publicationId)
        {
            release.PublicationId = publicationId;

            return await _releaseService
                .CreateRelease(release)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}")]
        public async Task<ActionResult<ReleaseViewModel>> DeleteRelease(Guid releaseId)
        {
            return await _releaseService
                .DeleteRelease(releaseId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId:guid}/amendment")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseAmendment(Guid releaseId)
        {
            return await _releaseService
                .CreateReleaseAmendment(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/data/{fileId:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataFileInfo>> GetDataFileInfo(Guid releaseId, Guid fileId)
        {
            return await _releaseDataFileService
                .GetInfo(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<DataFileInfo>>> GetDataFileInfo(Guid releaseId)
        {
            return await _releaseDataFileService
                .ListAll(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId:guid}/data/order")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<DataFileInfo>>> ReorderDataFiles(Guid releaseId,
            List<Guid> fileIds)
        {
            return await _releaseDataFileService
                .ReorderDataFiles(releaseId, fileIds)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/data")]
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
            return await _releaseDataFileService
                .Upload(releaseId: releaseId,
                    dataFormFile: file,
                    metaFormFile: metaFile,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/zip-data")]
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
            return await _releaseDataFileService
                .UploadAsZip(releaseId: releaseId,
                    zipFormFile: zipFile,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(Guid releaseId)
        {
            return await _releaseService
                .GetRelease(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/status")]
        public async Task<ActionResult<List<ReleaseStatusViewModel>>> GetReleaseStatuses(Guid releaseId)
        {
            return await _releaseApprovalService
                .GetReleaseStatuses(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseId)
        {
            return await _releaseService
                .GetReleasePublicationStatus(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId:guid}")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateRelease(ReleaseUpdateRequest request,
            Guid releaseId)
        {
            return await _releaseService
                .UpdateRelease(releaseId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("releases/{releaseId:guid}/status")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseStatus(ReleaseStatusCreateRequest request,
            Guid releaseId)
        {
            return await _releaseApprovalService
                .CreateReleaseStatus(releaseId, request)
                .OnSuccess(_ => _releaseService.GetRelease(releaseId))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId:guid}/releases/template")]
        public async Task<ActionResult<IdTitleViewModel>> GetTemplateRelease(
            [Required] Guid publicationId)
        {
            return await _releaseService
                .GetLatestPublishedRelease(publicationId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/draft")]
        public async Task<ActionResult<List<ReleaseViewModel>>> ListDraftReleases()
        {
            return await _releaseService
                .ListReleasesWithStatuses(ReleaseApprovalStatus.Draft, ReleaseApprovalStatus.HigherLevelReview)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<ReleaseViewModel>>> ListScheduledReleases()
        {
            return await _releaseService
                .ListScheduledReleases()
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/data/{fileId:guid}/import/status")]
        public Task<ActionResult<DataImportStatusViewModel>> GetDataUploadStatus(Guid releaseId, Guid fileId)
        {
            return _releaseService
                .GetDataFileImportStatus(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/delete-plan")]
        public async Task<ActionResult<DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseId)
        {
            return await _releaseService
                .GetDeleteReleasePlan(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/data/{fileId:guid}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseId:guid}/data/{fileId:guid}")]
        public async Task<ActionResult> DeleteDataFiles(Guid releaseId, Guid fileId)
        {
            return await _releaseService
                .RemoveDataFiles(releaseId, fileId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseId:guid}/data/{fileId:guid}/import/cancel")]
        public async Task<ActionResult> CancelFileImport(Guid releaseId, Guid fileId)
        {
            return await _dataImportService
                .CancelImport(releaseId, fileId)
                .HandleFailuresOr(result => new AcceptedResult());
        }

        [HttpGet("releases/{releaseId:guid}/stage-status")]
        public async Task<ActionResult<ReleasePublishingStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _releasePublishingStatusService
                .GetReleaseStatusAsync(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/checklist")]
        public async Task<ActionResult<ReleaseChecklistViewModel>> GetChecklist(Guid releaseId)
        {
            return await _releaseChecklistService
                .GetChecklist(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPatch("releases/{releaseId:guid}/published")]
        public async Task<ActionResult> UpdateReleasePublished(Guid releaseId,
            ReleasePublishedUpdateRequest request)
        {
            return await _releaseService
                .UpdateReleasePublished(releaseId, request)
                .HandleFailuresOrNoContent();
        }
    }
}
