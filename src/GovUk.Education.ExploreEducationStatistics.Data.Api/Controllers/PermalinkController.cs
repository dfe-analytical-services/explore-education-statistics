using System;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;

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
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var permalink = await _permalinkService.GetAsync(id);
                return Ok(permalink);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TableBuilderQueryContext query)
        {
            var permalink = await _permalinkService.CreateAsync(query);
            return Ok(permalink);
        }
    }
}