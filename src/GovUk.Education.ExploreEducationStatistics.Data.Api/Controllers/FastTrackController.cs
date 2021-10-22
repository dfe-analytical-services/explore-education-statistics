using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<FastTrackViewModel>> Get(string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _fastTrackService
                    .GetReleaseFastTrack(idAsGuid)
                    .OnSuccess(Get)
                    .HandleFailuresOrOk();
            }

            return NotFound();
        }
        
        [BlobCache(typeof(FastTrackResultsCacheKey))]
        private async Task<Either<ActionResult, FastTrackViewModel>> Get(ReleaseFastTrack fastTrack)
        {
            return await _fastTrackService.GetFastTrackAndResults(fastTrack.FastTrackId);
        }
    }
}