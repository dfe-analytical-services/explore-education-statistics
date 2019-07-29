namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    
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
        public  async Task<IActionResult> Get(Guid id)
        {
            var permalink = await _permalinkService.GetAsync(id);

            if (permalink == null)
            {
                return NotFound();
            }

            return Ok(permalink);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ObservationQueryContext tableQuery)
        {
            var permalink = await _permalinkService.CreateAsync(tableQuery);

            return Ok(permalink);
        }
    }
}