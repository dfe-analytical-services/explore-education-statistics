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
            return new GeographicModel
            {
                Year = 201617,
                Level = Level.National,
                Country = new Country
                {
                    Code = "E92000001",
                    Name = "England"
                },
                Region = new Region
                {
                    Code = "E13000001",
                    Name = "Inner London"
                },
                LocalAuthority = new LocalAuthority
                {
                    Name = "City of London",
                    Code = "E09000001",
                    Old_Code = "201"
                },
                laestab = "2013614",
                SchoolType = SchoolType.Primary
            };
        }
    }
}