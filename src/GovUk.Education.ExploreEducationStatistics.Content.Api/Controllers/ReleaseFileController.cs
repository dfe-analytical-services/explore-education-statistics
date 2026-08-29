#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[ApiController]
public class ReleaseFileController(
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IReleaseFileService releaseFileService
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
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionIdGuid)
                .OnSuccessDo(rv => this.CacheWithLastModified(rv.Published))
                .OnSuccess(rv => releaseFileService.StreamFile(releaseVersionId: rv.Id, fileId: fileIdGuid))
                .OnSuccessDo(result => this.CacheWithETag(result.FileStream.ComputeMd5Hash()))
                .HandleFailures();
        }

        return NotFound();
    }

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
        Response.Headers.CacheControl = "no-store";

        if (fileIds is not null && fileIds.Count > 1)
        {
            ModelState.AddModelError("fileIds", "Providing multiple fileIds is deprecated.");
            return BadRequest(ModelState);
        }

        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(
                releaseVersionId,
                q => q.Include(rv => rv.Release).ThenInclude(r => r.Publication)
            )
            .OnSuccess(releaseVersion =>
                releaseFileService.GetZipDelivery(releaseVersion, fromPage, fileIds, HttpContext.RequestAborted)
            )
            .OnSuccess(delivery => DeliverZip(delivery, fromPage, fileIds, HttpContext.RequestAborted))
            .OnFailureDo(result =>
            {
                Response.StatusCode = result is StatusCodeResult statusCodeResult ? statusCodeResult.StatusCode : 500;
            })
            .HandleFailuresOrNoOp();
    }

    private async Task<Either<ActionResult, Unit>> DeliverZip(
        ZipDelivery delivery,
        AnalyticsFromPage fromPage,
        IEnumerable<Guid>? fileIds,
        CancellationToken cancellationToken
    )
    {
        if (delivery is ZipDelivery.Redirect redirect)
        {
            Response.Redirect(redirect.Path);
            return Unit.Instance;
        }

        var streamDelivery = (ZipDelivery.Stream)delivery;

        Response.ContentDispositionAttachment(
            contentType: MediaTypeNames.Application.Octet,
            filename: $"{streamDelivery.ReleaseVersion.Release.Publication.Slug}_{streamDelivery.ReleaseVersion.Release.Slug}.zip"
        );

        return await releaseFileService.ZipFilesToStream(
            streamDelivery.ReleaseVersion.Id,
            Response.BodyWriter.AsStream(),
            fromPage,
            fileIds,
            cancellationToken
        );
    }

    [HttpGet("all-files/{releaseVersionId:guid}/v{formatVersion:int}")]
    [Produces(MediaTypeNames.Application.Zip)]
    public async Task<ActionResult> StreamCachedAllFilesZip(Guid releaseVersionId, int formatVersion)
    {
        var result = await releaseFileService.StreamCachedAllFilesZip(
            releaseVersionId,
            formatVersion,
            HttpContext.RequestAborted
        );

        if (result.IsLeft)
        {
            Response.Headers.CacheControl = "no-store";
            return result.Left;
        }

        // This is the only Content API response intended for AFD edge caching.
        // The versioned, query-free URL gives each ZIP format an unambiguous cache key.
        Response.Headers.CacheControl = "public, max-age=0, s-maxage=3600";
        return result.Right;
    }
}
