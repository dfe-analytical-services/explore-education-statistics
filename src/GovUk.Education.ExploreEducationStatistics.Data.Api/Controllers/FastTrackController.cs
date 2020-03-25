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

        [HttpGet("{id}")]
        public async Task<ActionResult<FastTrackViewModel>> GetAsync(Guid id)
        {
            return await _fastTrackService.GetAsync(id).HandleFailuresOrOk();
        }
    }
}