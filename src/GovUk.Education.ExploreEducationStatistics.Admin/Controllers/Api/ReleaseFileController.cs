#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
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

        [HttpDelete("release/{releaseId}/ancillary/{id}")]
        public async Task<ActionResult> DeleteFile(
            Guid releaseId, Guid id)
        {
            return await _releaseFileService
                .Delete(releaseId, id)
                .HandleFailuresOrNoContent();
        }

        [HttpDelete("release/{releaseId}/chart/{id}")]
        public async Task<ActionResult> DeleteChartFile(
            Guid releaseId, Guid id)
        {
            return await _dataBlockService.RemoveChartFile(releaseId, id)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("release/{releaseId:guid}/files")]
        [Produces(MediaTypeNames.Application.Octet)]
        public async Task StreamFilesToZip(
            Guid releaseId,
            [FromQuery] IList<Guid>? fileIds = null)
        {
            await _persistenceHelper.CheckEntityExists<Release>(
                    releaseId,
                    q => q.Include(r => r.Publication)
                )
                .OnSuccess(
                    async release =>
                    {
                        var filename = $"{release.Publication.Slug}_{release.Slug}.zip";
                        Response.Headers.Add(HeaderNames.ContentDisposition, @$"attachment; filename=""{filename}""");
                        Response.Headers.Add(HeaderNames.ContentType, MediaTypeNames.Application.Octet);

                        // We start the response immediately, before all of the files have
                        // even downloaded from blob storage. As we download them, they are
                        // appended in-flight to the user's download.
                        // This is more efficient and means the user doesn't have
                        // to spend time waiting for the download to initiate.
                        return await _releaseFileService.ZipFilesToStream(
                            releaseId: releaseId,
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

        [HttpGet("release/{releaseId}/file/{fileId}")]
        public async Task<ActionResult<FileInfo>> GetFile(Guid releaseId, Guid fileId)
        {
            return await _releaseFileService
                .GetFile(releaseId, fileId)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId}/file/{fileId}/download")]
        public async Task<ActionResult> Stream(Guid releaseId, Guid fileId)
        {
            return await _releaseFileService
                .Stream(releaseId: releaseId, fileId: fileId)
                .HandleFailures();
        }

        [HttpPatch("release/{releaseId}/file/{fileId}")]
        public async Task<ActionResult<Unit>> Update(
            Guid releaseId,
            Guid fileId,
            ReleaseFileUpdateViewModel update)
        {
            return await _releaseFileService
                .Update(releaseId: releaseId, fileId: fileId, update: update)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("release/{releaseId}/ancillary")]
        public async Task<ActionResult<IEnumerable<FileInfo>>> GetAncillaryFiles(Guid releaseId)
        {
            return await _releaseFileService
                .GetAncillaryFiles(releaseId)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/ancillary")]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> UploadAncillary(
            Guid releaseId,
            [FromForm] ReleaseAncillaryFileUploadViewModel upload)
        {
            return await _releaseFileService
                .UploadAncillary(releaseId, upload)
                .HandleFailuresOrOk();
        }

        [HttpPut("release/{releaseId}/chart/{id}")]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> UpdateChartFile(Guid releaseId, Guid id, IFormFile file)
        {
            return await _releaseFileService
                .UploadChart(releaseId, file, replacingId: id)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}/chart")]
        [RequestSizeLimit(int.MaxValue)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<FileInfo>> UploadChart(Guid releaseId, IFormFile file)
        {
            return await _releaseFileService
                .UploadChart(releaseId, file)
                .HandleFailuresOrOk();
        }
    }
}
