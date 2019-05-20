using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public ActionResult GetMethedologyTree()
        {
            return new OkResult();
        }
    }
}