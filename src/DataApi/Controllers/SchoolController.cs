using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/school")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<GeographicModel>> List(int releaseId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{schoolId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int schoolId)
        {
            return new GeographicModel(201617, "National", new Country("E92000001", "England"), new Region("E13000001", "Inner London"), new LocalAuthority("City of London", "E09000001", "201"), "2013614", SchoolType.Primary);
        }
    }
}