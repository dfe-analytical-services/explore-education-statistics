using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases;

[Route("api")]
[ApiController]
[Authorize]
public class ReleaseVersionsController(
    IReleaseVersionService releaseVersionService,
    IReleaseAmendmentService releaseAmendmentService,
    IReleaseApprovalService releaseApprovalService,
    IReleaseDataFileService releaseDataFileService,
    IReleasePublishingStatusService releasePublishingStatusService,
    IReleaseChecklistService releaseChecklistService,
    IDataImportService dataImportService,
    IDataSetUploadRepository dataSetUploadRepository,
    IDataSetFileStorage dataSetFileStorage,
    IDataSetScreenerService dataSetScreenerService
) : ControllerBase
{
    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpDelete("release/{releaseVersionId:guid}")]
    public async Task<ActionResult> DeleteReleaseVersion(Guid releaseVersionId, CancellationToken cancellationToken)
    {
        return await releaseVersionService
            .DeleteReleaseVersion(releaseVersionId, cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpPost("release/{releaseVersionId:guid}/amendment")]
    public async Task<ActionResult<IdViewModel>> CreateReleaseAmendment(Guid releaseVersionId)
    {
        return await releaseAmendmentService.CreateReleaseAmendment(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpGet("releaseVersions/{releaseVersionId:guid}/data/{fileId:guid}")]
    public async Task<ActionResult<DataFileInfo>> GetDataFileInfo(Guid releaseVersionId, Guid fileId)
    {
        return await releaseDataFileService.GetInfo(releaseVersionId, fileId).HandleFailuresOrOk();
    }

    [HttpGet(
        "releaseVersions/{releaseVersionId:guid}/{fileType}/{dataSetUploadId:guid}/download-temporary-file/blob-token"
    )]
    public async Task<ActionResult<string>> GetTemporaryDataSetUploadFileBlobDownloadToken(
        Guid releaseVersionId,
        FileType fileType,
        Guid dataSetUploadId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetFileStorage
            .GetTemporaryFileDownloadToken(releaseVersionId, dataSetUploadId, fileType, cancellationToken)
            .OnSuccess(token => token.ToBase64JsonString())
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}/accoutrements-summary")]
    public async Task<ActionResult<DataSetAccoutrementsViewModel>> GetDataSetAccoutrementsSummary(
        Guid releaseVersionId,
        Guid fileId
    )
    {
        return await releaseDataFileService
            .GetAccoutrementsSummary(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOrOk();
    }

    [HttpGet("releaseVersions/{releaseVersionId:guid}/data")]
    public async Task<ActionResult<List<DataFileInfo>>> GetDataFileInfo(Guid releaseVersionId)
    {
        return await releaseDataFileService.ListAll(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpGet("releaseVersions/{releaseVersionId:guid}/uploads")]
    public async Task<ActionResult<List<DataSetUploadViewModel>>> GetDataSetUploads(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetUploadRepository.ListAll(releaseVersionId, cancellationToken).HandleFailuresOrOk();
    }

    [HttpDelete("releaseVersions/{releaseVersionId:guid}/upload/{dataSetUploadId:guid}")]
    public async Task<ActionResult> DeleteDataSetUpload(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        CancellationToken cancellationToken
    )
    {
        return await dataSetUploadRepository
            .Delete(releaseVersionId, dataSetUploadId, cancellationToken)
            .HandleFailuresOrNoContent();
    }

    [HttpGet("releaseVersions/{releaseVersionId:guid}/uploads/screener/progress")]
    public async Task<
        ActionResult<List<ScreenerProgressWithDataSetUploadIdViewModel>>
    > GetDataSetUploadScreenerProgress(Guid releaseVersionId, CancellationToken cancellationToken)
    {
        return await dataSetScreenerService
            .GetScreenerProgress(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpPut("release/{releaseVersionId:guid}/data/order")]
    public async Task<ActionResult<List<DataFileInfo>>> ReorderDataFiles(Guid releaseVersionId, List<Guid> fileIds)
    {
        return await releaseDataFileService.ReorderDataFiles(releaseVersionId, fileIds).HandleFailuresOrOk();
    }

    [HttpPost("releaseVersions/data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<List<DataSetUploadViewModel>>> UploadDataSet(
        [FromForm] UploadDataSetRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var dataFile = new ManagedStreamFormFile(request.DataFile);
        await using var metaFile = new ManagedStreamFormFile(request.MetaFile);

        return await releaseDataFileService
            .Upload(request.ReleaseVersionId, dataFile, metaFile, request.Title, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost("releaseVersions/zip-data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<List<DataSetUploadViewModel>>> UploadDataSetAsZip(
        [FromForm] UploadDataSetAsZipRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var zipFile = new ManagedStreamZipFormFile(request.ZipFile);

        return await releaseDataFileService
            .UploadFromZip(request.ReleaseVersionId, zipFile, request.Title, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost("releaseVersions/upload-bulk-zip-data")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<List<DataSetUploadViewModel>>> UploadDataSetAsBulkZip(
        [FromForm] UploadDataSetAsBulkZipRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var zipFile = new ManagedStreamZipFormFile(request.ZipFile);

        return await releaseDataFileService
            .UploadFromBulkZip(request.ReleaseVersionId, zipFile, cancellationToken)
            .HandleFailuresOrOk();
    }

    [HttpPost("releaseVersions/{releaseVersionId:guid}/import-data-sets")]
    public async Task<ActionResult> ImportDataSetsFromTempStorage(
        Guid releaseVersionId,
        List<Guid> dataSetUploadIds,
        CancellationToken cancellationToken
    )
    {
        return await releaseDataFileService
            .SaveDataSetsFromTemporaryBlobStorage(releaseVersionId, dataSetUploadIds, cancellationToken)
            .HandleFailuresOrNoContent(convertNotFoundToNoContent: false);
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/{releaseVersionId:guid}")]
    public async Task<ActionResult<ReleaseVersionViewModel>> GetReleaseVersion(Guid releaseVersionId)
    {
        return await releaseVersionService.GetRelease(releaseVersionId).HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/{releaseVersionId:guid}/status")]
    public async Task<ActionResult<List<ReleaseStatusViewModel>>> ListReleaseStatuses(Guid releaseVersionId)
    {
        return await releaseApprovalService.ListReleaseStatuses(releaseVersionId).HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/{releaseVersionId:guid}/publication-status")]
    public async Task<ActionResult<ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(
        Guid releaseVersionId
    )
    {
        return await releaseVersionService.GetReleasePublicationStatus(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpPatch("releaseVersions/{releaseVersionId:guid}")]
    public async Task<ActionResult<ReleaseVersionViewModel>> UpdateReleaseVersion(
        ReleaseVersionUpdateRequest request,
        Guid releaseVersionId
    )
    {
        return await releaseVersionService.UpdateReleaseVersion(releaseVersionId, request).HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpPost("releases/{releaseVersionId:guid}/status")]
    public async Task<ActionResult<ReleaseVersionViewModel>> CreateReleaseStatus(
        ReleaseStatusCreateRequest request,
        Guid releaseVersionId
    )
    {
        return await releaseApprovalService
            .CreateReleaseStatus(releaseVersionId, request)
            .OnSuccess(_ => releaseVersionService.GetRelease(releaseVersionId))
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("publications/{publicationId:guid}/releases/template")]
    public async Task<ActionResult<IdTitleViewModel>> GetTemplateRelease([Required] Guid publicationId)
    {
        return await releaseVersionService.GetLatestPublishedRelease(publicationId).HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/draft")]
    public async Task<ActionResult<List<ReleaseVersionSummaryViewModel>>> ListDraftReleases()
    {
        return await releaseVersionService
            .ListReleasesWithStatuses(ReleaseApprovalStatus.Draft, ReleaseApprovalStatus.HigherLevelReview)
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/approvals")]
    public async Task<ActionResult<List<ReleaseVersionSummaryViewModel>>> ListUsersReleasesForApproval()
    {
        return await releaseVersionService.ListUsersReleasesForApproval().HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/scheduled")]
    public async Task<ActionResult<List<ReleaseVersionSummaryViewModel>>> ListScheduledReleases()
    {
        return await releaseVersionService.ListScheduledReleases().HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}/import/status")]
    public Task<ActionResult<DataImportStatusViewModel>> GetDataUploadStatus(Guid releaseVersionId, Guid fileId)
    {
        return releaseVersionService
            .GetDataFileImportStatus(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("release/{releaseVersionId:guid}/delete-plan")]
    public async Task<ActionResult<DeleteReleasePlanViewModel>> GetDeleteReleaseVersionPlan(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await releaseVersionService
            .GetDeleteReleaseVersionPlan(releaseVersionId, cancellationToken)
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("release/{releaseVersionId:guid}/data/{fileId:guid}/delete-plan")]
    public async Task<ActionResult<DeleteDataFilePlanViewModel>> GetDeleteDataFilePlan(
        Guid releaseVersionId,
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        return await releaseVersionService
            .GetDeleteDataFilePlan(
                releaseVersionId: releaseVersionId,
                fileId: fileId,
                cancellationToken: cancellationToken
            )
            .HandleFailuresOrOk();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpDelete("release/{releaseVersionId:guid}/data/{fileId:guid}")]
    public async Task<ActionResult> DeleteDataFiles(Guid releaseVersionId, Guid fileId)
    {
        return await releaseVersionService
            .RemoveDataFiles(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOrNoContent();
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpPost("release/{releaseVersionId:guid}/data/{fileId:guid}/import/cancel")]
    public async Task<ActionResult> CancelFileImport(Guid releaseVersionId, Guid fileId)
    {
        return await dataImportService
            .CancelImport(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOr(_ => new AcceptedResult());
    }

    // We intend to change this route, to make these endpoints more consistent, as per EES-5895
    [HttpGet("releases/{releaseVersionId:guid}/stage-status")]
    public async Task<ActionResult<ReleasePublishingStatusViewModel>> GetReleaseStatusesAsync(Guid releaseVersionId)
    {
        return await releasePublishingStatusService.GetReleaseStatusAsync(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpGet("releaseVersions/{releaseVersionId:guid}/checklist")]
    public async Task<ActionResult<ReleaseChecklistViewModel>> GetChecklist(Guid releaseVersionId)
    {
        return await releaseChecklistService.GetChecklist(releaseVersionId).HandleFailuresOrOk();
    }

    [HttpPatch("releaseVersions/{releaseVersionId:guid}/prerelease-access-list")]
    public async Task<ActionResult<ReleaseVersionViewModel>> UpdatePreReleaseAccessList(
        Guid releaseVersionId,
        ReleaseVersionPreReleaseAccessListUpdateRequest request,
        CancellationToken cancellationToken = default
    ) =>
        await releaseVersionService
            .UpdatePreReleaseAccessList(releaseVersionId, request, cancellationToken)
            .OnSuccess(_ => releaseVersionService.GetRelease(releaseVersionId))
            .HandleFailuresOrOk();

    [HttpPatch("releaseVersions/{releaseVersionId:guid}/published-display-date")]
    public async Task<ActionResult> UpdatePublishedDisplayDate(
        Guid releaseVersionId,
        ReleaseVersionPublishedDisplayDateUpdateRequest request
    )
    {
        return await releaseVersionService
            .UpdatePublishedDisplayDate(releaseVersionId, request)
            .HandleFailuresOrNoContent();
    }
}
