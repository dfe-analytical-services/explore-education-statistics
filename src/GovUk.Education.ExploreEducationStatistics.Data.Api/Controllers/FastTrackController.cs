using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
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
        private readonly ICacheKeyService _cacheKeyService;

        public FastTrackController(
            IFastTrackService fastTrackService, 
            ICacheKeyService cacheKeyService)
        {
            _fastTrackService = fastTrackService;
            _cacheKeyService = cacheKeyService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FastTrackViewModel>> Get(string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _cacheKeyService
                    .CreateCacheKeyForFastTrackResults(idAsGuid)
                    .OnSuccess(GetAndCacheFastTrackAndResults)
                    .HandleFailuresOrOk();
            }

            return NotFound();
        }

        [BlobCache(typeof(FastTrackResultsCacheKey))]
        private Task<Either<ActionResult, FastTrackViewModel>> GetAndCacheFastTrackAndResults(FastTrackResultsCacheKey cacheKey)
        {
            return _fastTrackService.GetFastTrackAndResults(cacheKey.FastTrackId);
        }
    }
}
