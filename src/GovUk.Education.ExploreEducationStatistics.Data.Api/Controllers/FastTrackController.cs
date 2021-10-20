using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache;
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

        [HttpGet("{id}")]
        [BlobCache(typeof(FastTrackResultsCacheKey))]
        public async Task<ActionResult<FastTrackViewModel>> Get(string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _fastTrackService.Get(idAsGuid).HandleFailuresOrOk();
            }

            return NotFound();
        }
    }
}