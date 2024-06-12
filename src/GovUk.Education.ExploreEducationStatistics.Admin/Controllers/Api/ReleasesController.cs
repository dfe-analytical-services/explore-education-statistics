using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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
        private readonly IReleaseAmendmentService _releaseAmendmentService;
        private readonly IReleaseApprovalService _releaseApprovalService;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IReleaseChecklistService _releaseChecklistService;
        private readonly IDataImportService _dataImportService;

        public ReleasesController(
            IReleaseService releaseService,
            IReleaseAmendmentService releaseAmendmentService,
            IReleaseApprovalService releaseApprovalService,
            IReleaseDataFileService releaseDataFileService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IReleaseChecklistService releaseChecklistService,
            IDataImportService dataImportService)
        {
            _releaseService = releaseService;
            _releaseAmendmentService = releaseAmendmentService;
            _releaseApprovalService = releaseApprovalService;
            _releaseDataFileService = releaseDataFileService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _releaseChecklistService = releaseChecklistService;
            _dataImportService = dataImportService;
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

        [HttpDelete("release/{releaseVersionId:guid}")]
        public async Task<ActionResult> DeleteRelease(Guid releaseVersionId)
        {
            return await _releaseService
                .DeleteRelease(releaseVersionId)
                .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
        }

        [HttpPost("release/{releaseVersionId:guid}/amendment")]
        public async Task<ActionResult<IdViewModel>> CreateReleaseAmendment(Guid releaseVersionId)
        {
            return await _releaseAmendmentService
                .CreateReleaseAmendment(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DataFileInfo>> GetDataFileInfo(Guid releaseVersionId, Guid fileId)
        {
            return await _releaseDataFileService
                .GetInfo(releaseVersionId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseVersionId:guid}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<DataFileInfo>>> GetDataFileInfo(Guid releaseVersionId)
        {
            return await _releaseDataFileService
                .ListAll(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseVersionId:guid}/data/order")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<DataFileInfo>>> ReorderDataFiles(Guid releaseVersionId,
            List<Guid> fileIds)
        {
            return await _releaseDataFileService
                .ReorderDataFiles(releaseVersionId, fileIds)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseVersionId:guid}/data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataFilesAsync(Guid releaseVersionId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "title")] string subjectName,
            IFormFile file,
            IFormFile metaFile)
        {
            return await _releaseDataFileService
                .Upload(releaseVersionId: releaseVersionId,
                    dataFormFile: file,
                    metaFormFile: metaFile,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseVersionId:guid}/zip-data")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<DataFileInfo>> AddDataZipFileAsync(Guid releaseVersionId,
            [FromQuery(Name = "replacingFileId")] Guid? replacingFileId,
            [FromQuery(Name = "title")] string subjectName,
            IFormFile zipFile)
        {
            return await _releaseDataFileService
                .UploadAsZip(releaseVersionId: releaseVersionId,
                    zipFormFile: zipFile,
                    replacingFileId: replacingFileId,
                    subjectName: subjectName)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseVersionId:guid}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(Guid releaseVersionId)
        {
            return await _releaseService
                .GetRelease(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseVersionId:guid}/status")]
        public async Task<ActionResult<List<ReleaseStatusViewModel>>> ListReleaseStatuses(Guid releaseVersionId)
        {
            return await _releaseApprovalService
                .ListReleaseStatuses(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseVersionId:guid}/publication-status")]
        public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
            Guid releaseVersionId)
        {
            return await _releaseService
                .GetReleasePublicationStatus(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseVersionId:guid}")]
        public async Task<ActionResult<ReleaseViewModel>> UpdateRelease(ReleaseUpdateRequest request,
            Guid releaseVersionId)
        {
            return await _releaseService
                .UpdateReleaseVersion(releaseVersionId, request)
                .HandleFailuresOrOk();
        }

        [HttpPost("releases/{releaseVersionId:guid}/status")]
        public async Task<ActionResult<ReleaseViewModel>> CreateReleaseStatus(ReleaseStatusCreateRequest request,
            Guid releaseVersionId)
        {
            return await _releaseApprovalService
                .CreateReleaseStatus(releaseVersionId, request)
                .OnSuccess(_ => _releaseService.GetRelease(releaseVersionId))
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
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListDraftReleases()
        {
            return await _releaseService
                .ListReleasesWithStatuses(ReleaseApprovalStatus.Draft, ReleaseApprovalStatus.HigherLevelReview)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/approvals")]
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListUsersReleasesForApproval()
        {
            return await _releaseService
                .ListUsersReleasesForApproval()
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/scheduled")]
        public async Task<ActionResult<List<ReleaseSummaryViewModel>>> ListScheduledReleases()
        {
            return await _releaseService
                .ListScheduledReleases()
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}/import/status")]
        public Task<ActionResult<DataImportStatusViewModel>> GetDataUploadStatus(Guid releaseVersionId, Guid fileId)
        {
            return _releaseService
                .GetDataFileImportStatus(releaseVersionId: releaseVersionId,
                    fileId: fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseVersionId:guid}/delete-plan")]
        public async Task<ActionResult<DeleteReleasePlan>> GetDeleteReleasePlan(
            Guid releaseVersionId,
            CancellationToken cancellationToken)
        {
            return await _releaseService
                .GetDeleteReleasePlan(releaseVersionId, cancellationToken)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}/delete-plan")]
        public async Task<ActionResult<DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseVersionId, Guid fileId)
        {
            return await _releaseService
                .GetDeleteDataFilePlan(releaseVersionId: releaseVersionId,
                    fileId: fileId)
                .HandleFailuresOrOk();
        }

        [HttpDelete("release/{releaseVersionId:guid}/data/{fileId:guid}")]
        public async Task<ActionResult> DeleteDataFiles(Guid releaseVersionId, Guid fileId)
        {
            return await _releaseService
                .RemoveDataFiles(releaseVersionId: releaseVersionId,
                    fileId: fileId)
                .HandleFailuresOrNoContent();
        }

        [HttpPost("release/{releaseVersionId:guid}/data/{fileId:guid}/import/cancel")]
        public async Task<ActionResult> CancelFileImport(Guid releaseVersionId, Guid fileId)
        {
            return await _dataImportService
                .CancelImport(releaseVersionId: releaseVersionId,
                    fileId: fileId)
                .HandleFailuresOr(_ => new AcceptedResult());
        }

        [HttpGet("releases/{releaseVersionId:guid}/stage-status")]
        public async Task<ActionResult<ReleasePublishingStatusViewModel>> GetReleaseStatusesAsync(Guid releaseVersionId)
        {
            return await _releasePublishingStatusService
                .GetReleaseStatusAsync(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseVersionId:guid}/checklist")]
        public async Task<ActionResult<ReleaseChecklistViewModel>> GetChecklist(Guid releaseVersionId)
        {
            return await _releaseChecklistService
                .GetChecklist(releaseVersionId)
                .HandleFailuresOrOk();
        }

        [HttpPatch("releases/{releaseVersionId:guid}/published")]
        public async Task<ActionResult> UpdateReleasePublished(Guid releaseVersionId,
            ReleasePublishedUpdateRequest request)
        {
            return await _releaseService
                .UpdateReleasePublished(releaseVersionId, request)
                .HandleFailuresOrNoContent();
        }
    }
}
