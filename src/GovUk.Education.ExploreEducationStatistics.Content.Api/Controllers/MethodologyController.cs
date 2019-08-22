using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class MethodologyController : ControllerBase
    {
        private readonly IContentCacheService _contentCacheService;

        
        public MethodologyController(IContentCacheService contentCacheService)
        {
            _contentCacheService = contentCacheService;
        }

        // GET
        /// <response code="204">If the item is null</response>    
        [HttpGet("tree")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [Produces("application/json")]
        public async Task<ActionResult<List<ThemeTree>>> GetMethodologyTree()
        {
            var tree = await _contentCacheService.GetMethodologyTreeAsync();

            if (tree.Any())
            {
                return tree;
            }

            return NoContent();
        }
        
        // GET api/methodology/name-of-content
        [HttpGet("{slug}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [Produces("application/json")]
        public async Task<ActionResult<Methodology>> Get(string slug)
        {
            var methodology = await _contentCacheService.GetMethodologyAsync(slug);

            if (methodology != null)
            {
                return methodology;
            }
            
            return NotFound();
        }
    }
}