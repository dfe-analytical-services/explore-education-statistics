using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        [HttpGet("{releaseId}")]
        public ActionResult<string> Get(int releaseId)
        {
            return "value";
        }
    }
}