#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class ReleaseFileController : ControllerBase
{
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IDataBlockService _dataBlockService;
    private readonly IReleaseFileService _releaseFileService;

    public ReleaseFileController(
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IDataBlockService dataBlockService,
        IReleaseFileService releaseFileService)
    {
        _persistenceHelper = persistenceHelper;
        _dataBlockService = dataBlockService;
        _releaseFileService = releaseFileService;
    }

    [HttpDelete("release/{releaseVersionId:guid}/ancillary/{fileId:guid}")]
    public async Task<ActionResult> DeleteFile(
        Guid releaseVersionId,
        Guid fileId)
    {
        return await _releaseFileService
            .Delete(releaseVersionId: releaseVersionId, fileId: fileId)
            .HandleFailuresOrNoContent();
    }

    [HttpDelete("release/{releaseVersionId:guid}/chart/{fileId:guid}")]
    public async Task<ActionResult> DeleteChartFile(
        Guid releaseVersionId,
        Guid fileId)
    {
        return await _dataBlockService.RemoveChartFile(releaseVersionId, fileId)
            .HandleFailuresOrNoContent();
    }

    [HttpGet("release/{releaseVersionId:guid}/files")]
    [Produces(MediaTypeNames.Application.Octet)]
    public async Task<ActionResult> StreamFilesToZip(
        Guid releaseVersionId,
        [FromQuery] IList<Guid>? fileIds = null)
    {
        return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(
                releaseVersionId,
                q => q.Include(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
            )
            .OnSuccess(
                async releaseVersion =>
                {
                    Response.ContentDispositionAttachment(
                        contentType: MediaTypeNames.Application.Octet,
                        filename: $"{releaseVersion.Release.Publication.Slug}_{releaseVersion.Release.Slug}.zip");

                    // We start the response immediately, before all of the files have
                    // even downloaded from blob storage. As we download them, they are
                    // appended in-flight to the user's download.
                    // This is more efficient and means the user doesn't have
                    // to spend time waiting for the download to initiate.
                    return await _releaseFileService.ZipFilesToStream(
                        releaseVersionId: releaseVersionId,
                        outputStream: Response.BodyWriter.AsStream(),
                        fileIds: fileIds,
                        cancellationToken: HttpContext.RequestAborted
                    );
                }
            )
            .OnFailureDo(
                result =>
                {
                    Response.StatusCode = result is StatusCodeResult statusCodeResult
                        ? statusCodeResult.StatusCode
                        : 500;
                }
            )
            .HandleFailuresOrNoOp();
    }

    [HttpGet("release/{releaseVersionId:guid}/file/{fileId:guid}")]
    public async Task<ActionResult<FileInfo>> GetFile(Guid releaseVersionId,
        Guid fileId)
    {
        return await _releaseFileService
            .GetFile(releaseVersionId: releaseVersionId,
                fileId: fileId)
            .HandleFailuresOrOk();
    }

    [HttpGet("release/{releaseVersionId:guid}/file/{fileId:guid}/download")]
    public async Task<ActionResult> Stream(Guid releaseVersionId,
        Guid fileId)
    {
        return await _releaseFileService
            .Stream(releaseVersionId: releaseVersionId,
                fileId: fileId)
            .HandleFailures();
    }

    [HttpPatch("release/{releaseVersionId:guid}/data/{fileId:guid}")]
    public async Task<ActionResult<Unit>> UpdateDataFileDetails(
        Guid releaseVersionId,
        Guid fileId,
        ReleaseDataFileUpdateRequest update)
    {
        return await _releaseFileService
            .UpdateDataFileDetails(releaseVersionId: releaseVersionId,
                fileId: fileId,
                update: update)
            .HandleFailuresOrNoContent();
    }

    [HttpGet("release/{releaseVersionId:guid}/ancillary")]
    public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseVersionId)
    {
        return await _releaseFileService
            .GetAncillaryFiles(releaseVersionId)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/ancillary")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<FileInfo>> UploadAncillary(
        Guid releaseVersionId,
        [FromForm] ReleaseAncillaryFileUploadRequest upload)
    {
        return await _releaseFileService
            .UploadAncillary(releaseVersionId, upload)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/ancillary/{fileId:guid}")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<FileInfo>> UpdateAncillary(
        Guid releaseVersionId,
        Guid fileId,
        [FromForm] ReleaseAncillaryFileUpdateRequest request)
    {
        return await _releaseFileService
            .UpdateAncillary(releaseVersionId: releaseVersionId,
                fileId: fileId,
                request)
            .HandleFailuresOrOk();
    }

    [HttpPut("release/{releaseVersionId:guid}/chart/{fileId:guid}")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<FileInfo>> UpdateChartFile(Guid releaseVersionId,
        Guid fileId,
        IFormFile file)
    {
        return await _releaseFileService
            .UploadChart(releaseVersionId: releaseVersionId,
                file,
                replacingId: fileId)
            .HandleFailuresOrOk();
    }

    [HttpPost("release/{releaseVersionId:guid}/chart")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<ActionResult<FileInfo>> UploadChart(Guid releaseVersionId, IFormFile file)
    {
        return await _releaseFileService
            .UploadChart(releaseVersionId, file)
            .HandleFailuresOrOk();
    }
}
