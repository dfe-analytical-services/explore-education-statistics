using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FastTrackController : ControllerBase
    {
        private readonly IFastTrackService _fastTrackService;

        public FastTrackController(IFastTrackService fastTrackService)
        {
            _fastTrackService = fastTrackService;
        }

        [HttpGet("release/{releaseId}/{id}")]
        public async Task<ActionResult<FastTrackViewModel>> GetAsync(Guid releaseId, string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _fastTrackService.GetAsync(releaseId, idAsGuid).HandleFailuresOrOk();
            }

            return NotFound();
        }
    }
}