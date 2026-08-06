#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
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
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[ApiController]
public class ReleaseFileController(
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IReleaseFileService releaseFileService,
    IOptions<DirectBlobDownloadsOptions> directBlobDownloadsOptions
) : ControllerBase
{
    [HttpPost("release-files")]
    public async Task<ActionResult<IList<ReleaseFileViewModel>>> ListReleaseFiles(
        [FromBody] ReleaseFileListRequest request,
        CancellationToken cancellationToken
    )
    {
        return await releaseFileService.ListReleaseFiles(request, cancellationToken).HandleFailuresOrOk();
    }

    [ResponseCache(Duration = 300)]
    [HttpGet("releases/{releaseVersionId}/files/{fileId}")]
    public async Task<ActionResult> Stream(string releaseVersionId, string fileId)
    {
        if (Guid.TryParse(releaseVersionId, out var releaseVersionIdGuid) && Guid.TryParse(fileId, out var fileIdGuid))
        {
            if (directBlobDownloadsOptions.Value.Enabled)
            {
                var redirectResult = await releaseFileService.GetFileDownloadRedirectPath(
                    releaseVersionIdGuid,
                    fileIdGuid,
                    HttpContext.RequestAborted
                );

                if (redirectResult.IsLeft)
                {
                    return redirectResult.Left;
                }

                if (redirectResult.Right is { } redirectPath)
                {
                    return Redirect(redirectPath);
                }
            }

            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionIdGuid)
                .OnSuccessDo(rv => this.CacheWithLastModified(rv.Published))
                .OnSuccess(rv => releaseFileService.StreamFile(releaseVersionId: rv.Id, fileId: fileIdGuid))
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
        // TODO EES-6034
        // The previous data catalogue page allowed users to selected multiple specific files to include in the
        // zip file, hence why this endpoint takes an array of fileIds, but this is no longer the case. Via the
        // public frontend, users only download all the releaseVersion's data (by not providing fileIds) or provide
        // a single fileId for a specific data set.
        [FromQuery] AnalyticsFromPage fromPage,
        [FromQuery] IList<Guid>? fileIds = null
    )
    {
        if (fileIds is not null && fileIds.Count > 1)
        {
            ModelState.AddModelError("fileIds", "Providing multiple fileIds is deprecated.");
            return BadRequest(ModelState);
        }

        var releaseVersionResult = await persistenceHelper.CheckEntityExists<ReleaseVersion>(
            releaseVersionId,
            q => q.Include(rv => rv.Release).ThenInclude(r => r.Publication)
        );

        if (releaseVersionResult.IsLeft)
        {
            return releaseVersionResult.Left;
        }

        var releaseVersion = releaseVersionResult.Right;
        var cacheResult = await this.CacheWithLastModified(releaseVersion.Published);
        if (cacheResult.IsLeft)
        {
            return cacheResult.Left;
        }

        if (directBlobDownloadsOptions.Value.Enabled && fileIds is null)
        {
            var redirectResult = await releaseFileService.GetAllFilesZipDownloadRedirectPath(
                releaseVersion,
                fromPage,
                HttpContext.RequestAborted
            );

            if (redirectResult.IsLeft)
            {
                return redirectResult.Left;
            }

            if (redirectResult.Right is { } redirectPath)
            {
                return Redirect(redirectPath);
            }
        }

        Response.ContentDispositionAttachment(
            contentType: MediaTypeNames.Application.Octet,
            filename: $"{releaseVersion.Release.Publication.Slug}_{releaseVersion.Release.Slug}.zip"
        );

        // Start the response before all files have downloaded and append them in-flight.
        var streamResult = await releaseFileService.ZipFilesToStream(
            releaseVersionId: releaseVersionId,
            outputStream: Response.BodyWriter.AsStream(),
            fromPage: fromPage,
            fileIds: fileIds,
            cancellationToken: HttpContext.RequestAborted
        );

        if (streamResult.IsLeft)
        {
            Response.StatusCode = streamResult.Left is StatusCodeResult statusCodeResult
                ? statusCodeResult.StatusCode
                : 500;
            return streamResult.Left;
        }

        return new EmptyResult();
    }
}
