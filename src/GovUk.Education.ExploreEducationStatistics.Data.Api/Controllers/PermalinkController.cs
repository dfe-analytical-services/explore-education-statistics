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

        [HttpGet("permalink/{permalinkId:guid}")]
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

        [HttpPost("permalink")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PermalinkViewModel>> CreatePermalink(
            [FromBody] PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService
                .CreatePermalink(request, cancellationToken)
                .HandleFailuresOr(permalink => CreatedAtAction(nameof(GetPermalink), new
                {
                    permalinkId = permalink.Id
                }, permalink));
        }

        [HttpPost("permalink/release/{releaseId:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PermalinkViewModel>> CreatePermalink(
            Guid releaseId,
            [FromBody] PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _permalinkService
                .CreatePermalink(releaseId, request, cancellationToken)
                .HandleFailuresOr(permalink => CreatedAtAction(nameof(GetPermalink), new
                {
                    permalinkId = permalink.Id
                }, permalink));
        }
    }
}
