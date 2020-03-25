using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermalinkController : ControllerBase
    {
        private readonly IPermalinkService _permalinkService;

        public PermalinkController(IPermalinkService permalinkService)
        {
            _permalinkService = permalinkService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermalinkViewModel>> GetAsync(Guid id)
        {
            return await _permalinkService.GetAsync(id).HandleFailuresOrOk();
        }

        [HttpPost]
        public async Task<ActionResult<PermalinkViewModel>> CreateAsync([FromBody] TableBuilderQueryContext query)
        {
            return await _permalinkService.CreateAsync(query).HandleFailuresOrOk();
        }
    }
}