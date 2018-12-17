using System;
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
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Level == "School"
                ).ToList();
        }

        [HttpGet("{schoolId}")]
        public ActionResult<GeographicModel> Get(string publication, string schoolId,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
                .FirstOrDefault(x =>
                    (!year.HasValue || x.Year == year) &&
                    x.School.laestab == schoolId
                );
        }
    }
}