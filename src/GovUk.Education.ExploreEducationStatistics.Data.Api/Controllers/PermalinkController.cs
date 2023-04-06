#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class PermalinkController : ControllerBase
    {
        private readonly IPermalinkService _permalinkService;

        public PermalinkController(IPermalinkService permalinkService)
        {
            _permalinkService = permalinkService;
        }
        
        // Legacy endpoints
        [HttpGet("permalink/{id:guid}")]
        [Produces("application/json", "text/csv")]
        public async Task GetLegacyPermalink(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(
                    contentType: "text/csv",
                    filename: $"permalink-{id}.csv");

                await _permalinkService.LegacyDownloadCsvToStream(id, Response.BodyWriter.AsStream(),
                    cancellationToken);

                return;
            }

            var result = await _permalinkService
                .GetLegacy(id, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        [HttpPost]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink(
            [FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(request).HandleFailuresOrOk();
        }

        [HttpPost("permalink/release/{releaseId:guid}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink(Guid releaseId,
            [FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(releaseId, request).HandleFailuresOrOk();
        }

        // snapshot endpoints
        [HttpPost("permalink-snapshot/{id:guid}")]
        [ProducesResponseType(200)]
        public async Task<Either<ActionResult, PermalinkSnapshotViewModel>> Get(Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService.Get(id, cancellationToken);
        }
        
        [HttpPost("permalink-snapshot")]
        [ProducesResponseType(201)]
        public async Task<Either<ActionResult, PermalinkSnapshotViewModel>> Create(
            [FromBody] PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService.Create(request, cancellationToken);
        }
        
        [HttpGet("permalink-snapshot/{id:guid}/csv")]
        [Produces("text/csv")]
        // Correct status code?? streaming partial content?
        [ProducesResponseType(206)]
        public async Task<Either<ActionResult, Stream>> StreamPermalinkCsv(Guid id, Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService.StreamPermalinkCsv(id, stream, cancellationToken);
        }
    }
}