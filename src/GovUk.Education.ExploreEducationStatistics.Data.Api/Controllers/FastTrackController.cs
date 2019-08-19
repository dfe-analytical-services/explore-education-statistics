using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
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
        public async Task<IActionResult> Get(Guid id)
        {
            var viewModel = await _fastTrackService.GetAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            return Ok(viewModel);
        }
    }
}