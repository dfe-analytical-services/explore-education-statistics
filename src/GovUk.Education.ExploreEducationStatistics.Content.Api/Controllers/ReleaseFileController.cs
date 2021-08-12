#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("releases/{releaseId}/files/{fileId}")]
        public async Task<ActionResult> Stream(string releaseId, string fileId)
        {
            if (Guid.TryParse(releaseId, out var releaseIdAsGuid) &&
                Guid.TryParse(fileId, out var fileIdAsGuid))
            {
                return await _releaseFileService
                    .StreamFile(releaseIdAsGuid, fileIdAsGuid)
                    .HandleFailures();
            }

            return NotFound();
        }

        [HttpGet("releases/{releaseId}/files/all")]
        public async Task<ActionResult> StreamAll(string releaseId)
        {
            if (Guid.TryParse(releaseId, out var releaseIdAsGuid))
            {
                return await _releaseFileService
                    .StreamAllFilesZip(releaseIdAsGuid)
                    .HandleFailures();
            }

            return NotFound();
        }

        [HttpGet("releases/{releaseId}/files")]
        [Produces(MediaTypeNames.Application.Zip)]
        public async Task Stream(
            Guid releaseId,
            [FromQuery] IList<Guid> fileIds)
        {
            await _persistenceHelper.CheckEntityExists<Release>(
                    releaseId,
                    q => q.Include(r => r.Publication)
                )
                .OnSuccess(
                    async release =>
                    {
                        // Create a hash just so that we have some uniqueness
                        // to attach to the end of the file name.
                        var fileIdsHash = GetFileIdsHash(fileIds);

                        Response.Headers.Add(
                            "Content-Disposition",
                            $"attachment; filename={release.Publication.Slug}_{release.Slug}_{fileIdsHash}.zip"
                        );

                        // We start the response immediately, before all of the files have
                        // even downloaded from blob storage. As we download them, they are
                        // appended in-flight to the user's download.
                        // This is more efficient and means the user doesn't have
                        // to spend time waiting for the download to initiate.
                        return await _releaseFileService.ZipFilesToStream(
                            releaseId: releaseId,
                            fileIds: fileIds,
                            outputStream: Response.BodyWriter.AsStream(),
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

        private static string GetFileIdsHash(IList<Guid> fileIds)
        {
            return fileIds.Select(id => id.ToString())
                .OrderBy(id => id)
                .JoinToString(',')
                .ToMd5Hash();
        }
    }
}
