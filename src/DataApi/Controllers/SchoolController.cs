using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{publication}/geo-levels/school")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ICsvReader _csvReader;

        public SchoolController(ICsvReader csvReader)
        {
            _csvReader = csvReader;
        }

        [HttpGet]
        public ActionResult<List<GeographicModel>> List(string publication,
            [FromQuery(Name = "school-type")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels").Where(x => x.Level == "School").ToList();
        }

        [HttpGet("{schoolId}")]
        public ActionResult<GeographicModel> Get(string publication, string schoolId)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .FirstOrDefault(x => x.School.laestab == schoolId);
        }
    }
}