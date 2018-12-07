using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/school")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<string> List(int releaseId)
        {
            return "value";
        }

        [HttpGet("{schoolId}")]
        public ActionResult<string> Get(int releaseId, int schoolId)
        {
            return "value";
        }
    }
}