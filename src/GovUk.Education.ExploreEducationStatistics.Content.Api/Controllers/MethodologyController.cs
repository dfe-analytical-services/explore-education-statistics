using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        [ProducesResponseType(typeof(List<ThemeTree>),200)]
        [ProducesResponseType(204)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetMethodologyTree()
        {
            var tree = await _contentCacheService.GetMethodologyTreeAsync();

            if (string.IsNullOrWhiteSpace(tree))
            {
                return NoContent();
            }
            return tree;

        }
        
        // GET api/methodology/name-of-content
        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(Methodology), 200)]
        [ProducesResponseType(404)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Get(string slug)
        {
            var methodology = await _contentCacheService.GetMethodologyAsync(slug);
            
            if (string.IsNullOrWhiteSpace(methodology))
            {
                return NotFound();
            }
            return methodology;
        }
    }
}