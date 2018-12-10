using System;
using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{publication}/geo-levels/region")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly ICsvReader _csvReader;

        public RegionController(ICsvReader csvReader)
        {
            _csvReader = csvReader;
        }

        [HttpGet]
        public ActionResult<List<GeographicModel>> List(string publication,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Level.ToLower() == "region"
                ).ToList();
        }

        [HttpGet("{regionId}")]
        public ActionResult<GeographicModel> Get(string publication, string regionId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .FirstOrDefault(x =>
                    x.Region.Code == regionId
                );
        }

        [HttpGet("{regionId}/local-authorities")]
        public ActionResult<List<GeographicModel>> GetLocalAuthorities(string publication, string regionId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Region.Code == regionId &&
                    x.Level.ToLower() == "local authority"
                ).ToList();
        }

        [HttpGet("{regionId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(string publication, string regionId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Region.Code == regionId &&
                    x.Level.ToLower() == "school"
                    ).ToList();
        }
    }
}