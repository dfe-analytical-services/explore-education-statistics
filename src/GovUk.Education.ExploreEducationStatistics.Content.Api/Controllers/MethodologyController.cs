using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class MethodologyController : ControllerBase
    {
        private readonly IMethodologyService _service;
        public MethodologyController(IMethodologyService service)
        {
            _service = service;
        }

        // GET
        [HttpGet("tree")]
        public ActionResult<List<ThemeTree>> GetMethodologyTree()
        {
            var tree = _service.GetTree();

            if (tree.Any())
            {
                return tree;
            }

            return NoContent();
        }
        
        // GET api/methodology/name-of-content
        [HttpGet("{slug}")]
        public ActionResult<Methodology> Get(string slug)
        {
            var methodology = _service.Get(slug);

            if (methodology != null)
            {
                return methodology;
            }
            
            return NotFound();
        }
    }
}