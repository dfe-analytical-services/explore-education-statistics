using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/local-authority")]
    [ApiController]
    public class LocalAuthorityController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<string> List(int releaseId)
        {
            return "value";
        }

        [HttpGet("{localAuthorityId}")]
        public ActionResult<string> Get(int releaseId, int localAuthorityId)
        {
            return "value";
        }

        [HttpGet("{localAuthorityId}/schools")]
        public ActionResult<string> GetSchools(int releaseId, int localAuthorityId)
        {
            return "value";
        }
    }
}