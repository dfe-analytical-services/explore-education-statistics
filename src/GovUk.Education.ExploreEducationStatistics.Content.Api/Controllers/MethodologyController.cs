using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
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
        public ActionResult<List<ThemeTree>> GetMethedologyTree()
        {
            var tree = _service.GetTree();

            if (tree.Any())
            {
                return tree;
            }

            return NoContent();
        }
    }
}