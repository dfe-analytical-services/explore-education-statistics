using System.Collections.Generic;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/local-authority")]
    [ApiController]
    public class LocalAuthorityController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> List(int releaseId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return "value";
        }

        [HttpGet("{localAuthorityId}")]
        public ActionResult<string> Get(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return "value";
        }

        [HttpGet("{localAuthorityId}/schools")]
        public ActionResult<string> GetSchools(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return "value";
        }
    }
}