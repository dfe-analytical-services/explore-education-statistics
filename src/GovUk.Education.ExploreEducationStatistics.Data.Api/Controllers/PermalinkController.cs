#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

                await _permalinkService.LegacyDownloadCsvToStream(id, Response.BodyWriter.AsStream(), cancellationToken);

                return;
            }

            var result = await _permalinkService
                .GetLegacy(id, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        [HttpPost]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink([FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(request).HandleFailuresOrOk();
        }

        [HttpPost("permalink/release/{releaseId:guid}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> CreateLegacyPermalink(Guid releaseId,
            [FromBody] PermalinkCreateRequest request)
        {
            return await _permalinkService.CreateLegacy(releaseId, request).HandleFailuresOrOk();
        }
    }
}
