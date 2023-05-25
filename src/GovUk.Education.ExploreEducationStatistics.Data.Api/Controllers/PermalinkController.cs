#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        public async Task Get(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (Request.AcceptsCsv(exact: true))
            {
                Response.ContentDispositionAttachment(
                    contentType: "text/csv",
                    filename: $"permalink-{id}.csv");

                await _permalinkService.DownloadCsvToStream(id, Response.BodyWriter.AsStream(), cancellationToken);

                return;
            }

            var result = await _permalinkService
                .Get(id, cancellationToken)
                .HandleFailuresOr(Ok);

            await result.ExecuteResultAsync(ControllerContext);
        }

        [HttpPost]
        public async Task<ActionResult<LegacyPermalinkViewModel>> Create([FromBody] PermalinkCreateViewModel request)
        {
            return await _permalinkService.Create(request).HandleFailuresOrOk();
        }

        [HttpPost("permalink/release/{releaseId:guid}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> Create(Guid releaseId,
            [FromBody] PermalinkCreateViewModel request)
        {
            return await _permalinkService.Create(releaseId, request).HandleFailuresOrOk();
        }
    }
}
