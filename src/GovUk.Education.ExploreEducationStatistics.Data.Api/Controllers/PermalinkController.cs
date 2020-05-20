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
    public class PermalinkController : ControllerBase
    {
        private readonly IPermalinkService _permalinkService;

        public PermalinkController(IPermalinkService permalinkService)
        {
            _permalinkService = permalinkService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermalinkViewModel>> GetAsync(string id)
        {
            if (Guid.TryParse(id, out var idAsGuid))
            {
                return await _permalinkService.GetAsync(idAsGuid).HandleFailuresOrOk();
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PermalinkViewModel>> CreateAsync([FromBody] CreatePermalinkRequest request)
        {
            return await _permalinkService.CreateAsync(request).HandleFailuresOrOk();
        }
    }
}