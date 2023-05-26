#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Http;
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

        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        [HttpGet("permalink/{permalinkId:guid}")]
        [Produces("application/json", "text/csv")]
        public async Task GetLegacyPermalink(
            Guid permalinkId,
            CancellationToken cancellationToken)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(
                    contentType: ContentTypes.Csv,
                    filename: $"permalink-{permalinkId}.csv");

                var csvResult = await _permalinkService.LegacyDownloadCsvToStream(
                        permalinkId: permalinkId,
                        stream: Response.BodyWriter.AsStream(),
                        cancellationToken: cancellationToken
                    )
                    .HandleFailuresOr(Ok);

                if (csvResult is not OkObjectResult)
                {
                    await csvResult.ExecuteResultAsync(ControllerContext);
                }

                return;
            }

            var result = await _permalinkService
                .GetLegacy(permalinkId, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        [HttpPost("permalink")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink(
            [FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(request).HandleFailuresOrOk();
        }

        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        [HttpPost("permalink/release/{releaseId:guid}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink(Guid releaseId,
            [FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(releaseId, request).HandleFailuresOrOk();
        }

        [HttpGet("permalink-snapshot/{permalinkId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json", "text/csv")]
        public async Task GetPermalink(Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(
                    contentType: ContentTypes.Csv,
                    filename: $"permalink-{permalinkId}.csv");

                var csvResult = await _permalinkService.DownloadCsvToStream(
                        permalinkId: permalinkId,
                        stream: Response.BodyWriter.AsStream(),
                        cancellationToken: cancellationToken
                    )
                    .HandleFailuresOr(Ok);

                if (csvResult is not OkObjectResult)
                {
                    await csvResult.ExecuteResultAsync(ControllerContext);
                }

                return;
            }

            var result = await _permalinkService
                .GetPermalink(permalinkId, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        [HttpPost("permalink-snapshot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PermalinkViewModel>> CreatePermalink(
            [FromBody] PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService
                .CreatePermalink(request, cancellationToken)
                .HandleFailuresOrOk();
        }

        [HttpPost("permalink-snapshot/release/{releaseId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PermalinkViewModel>> CreatePermalink(
            Guid releaseId,
            [FromBody] PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService
                .CreatePermalink(releaseId, request, cancellationToken)
                .HandleFailuresOrOk();
        }

        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        [HttpPut("permalink/{permalinkId:guid}/snapshot")]
        public async Task<ActionResult<Unit>> MigratePermalink(Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService
                .MigratePermalink(permalinkId, cancellationToken)
                .HandleFailuresOrOk();
        }
    }
}
