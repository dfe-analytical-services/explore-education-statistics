using System.Collections.Generic;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/school")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> List(int releaseId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
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