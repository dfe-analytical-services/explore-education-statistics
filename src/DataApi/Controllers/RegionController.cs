using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/region")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<string> List(int releaseId)
        {
            return "value";
        }

        [HttpGet("{regionId}")]
        public ActionResult<string> Get(int releaseId, int regionId)
        {
            return "value";
        }

        [HttpGet("{regionId}/local-authorities")]
        public ActionResult<string> GetLocalAuthorities(int releaseId, int regionId)
        {
            return "value";
        }

        [HttpGet("{regionId}/schools")]
        public ActionResult<string> GetSchools(int releaseId, int regionId)
        {
            return "value";
        }
    }
}