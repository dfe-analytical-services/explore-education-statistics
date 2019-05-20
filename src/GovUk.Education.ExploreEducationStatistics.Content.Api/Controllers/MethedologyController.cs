using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class MethedologyController : Controller
    {
        private readonly IMethedologyService _service;
        public MethedologyController(IMethedologyService service)
        {
            _service = service;
        }

        // GET
        [HttpGet("tree")]
        public IActionResult GetMethedologyTree()
        {
            return new OkResult();
        }
    }
}