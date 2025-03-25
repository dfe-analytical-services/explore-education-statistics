#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ReleaseFileController(
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IReleaseFileService releaseFileService,
        IAnalyticsManager analyticsManager,
        ILogger<ReleaseFileController> logger)
        : ControllerBase
    {
        [HttpPost("release-files")]
        public async Task<ActionResult<IList<ReleaseFileViewModel>>> ListReleaseFiles(
            [FromBody] ReleaseFileListRequest request,
            CancellationToken cancellationToken)
        {
            return await releaseFileService
                .ListReleaseFiles(request, cancellationToken)
                .HandleFailuresOrOk();
        }

        [ResponseCache(Duration = 300)]
        [HttpGet("releases/{releaseVersionId}/files/{fileId}")] // @MarkFix what is this being used for?
        public async Task<ActionResult> Stream(string releaseVersionId, string fileId)
        {
            if (Guid.TryParse(releaseVersionId, out var releaseVersionIdGuid) &&
                Guid.TryParse(fileId, out var fileIdGuid))
            {
                return await persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionIdGuid)
                    .OnSuccessDo(rv => this.CacheWithLastModified(rv.Published))
                    .OnSuccess(rv => releaseFileService.StreamFile(releaseVersionId: rv.Id,
                        fileId: fileIdGuid))
                    .OnSuccessDo(result => this.CacheWithETag(result.FileStream.ComputeMd5Hash()))
                    .HandleFailures();
            }

            return NotFound();
        }

        [ResponseCache(Duration = 300)]
        [HttpGet("releases/{releaseVersionId:guid}/files")]
        [Produces(MediaTypeNames.Application.Octet)]
        public async Task<ActionResult> StreamFilesToZip(
            Guid releaseVersionId,
            CancellationToken cancellationToken,
            [FromQuery] IList<Guid>? fileIds = null)
        {
            return await persistenceHelper.CheckEntityExists<ReleaseVersion>(
                    releaseVersionId,
                    q => q.Include(rv => rv.Release)
                        .ThenInclude(r => r.Publication)
                )
                .OnSuccessDo(releaseVersion => this.CacheWithLastModified(releaseVersion.Published))
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
                        return await releaseFileService.ZipFilesToStream(
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
                .OnSuccessDo(_ =>
                {
                    try
                    {
                        analyticsManager.AddReleaseVersionZipDownload(
                            new AnalyticsManager.CaptureReleaseVersionZipDownloadRequest(releaseVersionId, fileIds),
                            cancellationToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(
                            exception: e,
                            message: "Error whilst adding a ReleaseVersionZipDownload event to {AnalyticsManager}",
                            nameof(IAnalyticsManager));
                    }
                })
                .HandleFailuresOrNoOp();
        }
    }
}
