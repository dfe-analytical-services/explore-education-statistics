#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ReleaseFileController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IReleaseFileService _releaseFileService;

        public ReleaseFileController(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IReleaseFileService releaseFileService)
        {
            _persistenceHelper = persistenceHelper;
            _releaseFileService = releaseFileService;
        }

        [ResponseCache(Duration = 300)]
        [HttpGet("releases/{releaseVersionId}/files/{fileId}")]
        public async Task<ActionResult> Stream(string releaseVersionId, string fileId)
        {
            if (Guid.TryParse(releaseVersionId, out var releaseVersionIdGuid) &&
                Guid.TryParse(fileId, out var fileIdGuid))
            {
                return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionIdGuid)
                    .OnSuccessDo(rv => this.CacheWithLastModified(rv.Published))
                    .OnSuccess(rv =>  _releaseFileService.StreamFile(releaseVersionId: rv.Id,
                        fileId: fileIdGuid))
                    .OnSuccessDo(result => this.CacheWithETag(result.FileStream.ComputeMd5Hash()))
                    .HandleFailures();
            }

            return NotFound();
        }

        [ResponseCache(Duration = 300)]
        [HttpGet("releases/{releaseVersionId:guid}/files")]
        [Produces(MediaTypeNames.Application.Octet)]
        public async Task StreamFilesToZip(
            Guid releaseVersionId,
            [FromQuery] IList<Guid>? fileIds = null)
        {
            await _persistenceHelper.CheckEntityExists<ReleaseVersion>(
                    releaseVersionId,
                    q => q.Include(rv => rv.Publication)
                )
                .OnSuccessDo(releaseVersion => this.CacheWithLastModified(releaseVersion.Published))
                .OnSuccess(
                    async releaseVersion =>
                    {
                        var filename = $"{releaseVersion.Publication.Slug}_{releaseVersion.Slug}.zip";
                        Response.Headers.Add(HeaderNames.ContentDisposition, @$"attachment; filename=""{filename}""");
                        Response.Headers.Add(HeaderNames.ContentType, MediaTypeNames.Application.Octet);

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
                .OnFailureVoid(
                    result =>
                    {
                        if (result is StatusCodeResult statusCodeResult)
                        {
                            Response.StatusCode = statusCodeResult.StatusCode;
                        }
                        else
                        {
                            Response.StatusCode = 500;
                        }
                    }
                );

        }
    }
}
