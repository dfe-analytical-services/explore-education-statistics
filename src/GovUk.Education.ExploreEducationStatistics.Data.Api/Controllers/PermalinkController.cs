#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/permalink")]
    [ApiController]
    public class PermalinkController : ControllerBase
    {
        private readonly IPermalinkService _permalinkService;

        public PermalinkController(IPermalinkService permalinkService)
        {
            _permalinkService = permalinkService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> Get(string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _permalinkService.Get(idAsGuid).HandleFailuresOrOk();
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<LegacyPermalinkViewModel>> Create([FromBody] PermalinkCreateViewModel request)
        {
            return await _permalinkService.Create(request).HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId}")]
        public async Task<ActionResult<LegacyPermalinkViewModel>> Create(Guid releaseId,
            [FromBody] PermalinkCreateViewModel request)
        {
            return await _permalinkService.Create(releaseId, request).HandleFailuresOrOk();
        }
    }
}
