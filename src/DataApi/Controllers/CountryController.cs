using System;
using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace DataApi.Controllers
{
    [Route("data/{publication}/geo-levels/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<GeographicModel>> List(string publication,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new CsvReader().GeoLevels(publication + "_geoglevels").Where(x => x.Level.ToLower() == "national").ToList();
        }

        [HttpGet("{countryId}")]
        public ActionResult<GeographicModel> Get(string publication, int releaseId, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new CsvReader().GeoLevels(publication + "_geoglevels").FirstOrDefault(x => x.Country.Code == countryId && x.Level.ToLower() == "national");
        }

        [HttpGet("{countryId}/regions")]
        public ActionResult<List<GeographicModel>> GetRegions(string publication,  string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new CsvReader().GeoLevels(publication + "_geoglevels").Where(x => x.Country.Code == countryId && x.Level.ToLower() == "region").ToList();
        }

        [HttpGet("{countryId}/local-authorities")]
        public ActionResult<List<GeographicModel>> GetLocalAuthorities(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new CsvReader().GeoLevels(publication + "_geoglevels").Where(x => x.Country.Code == countryId && x.Level.ToLower() == "local authority")
                .ToList();
        }

        [HttpGet("{countryId}/schools")]
        public ActionResult<List<GeographicModel>> getSchools(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new CsvReader().GeoLevels(publication + "_geoglevels").Where(x => x.Country.Code == countryId && x.Level.ToLower() == "school").ToList();
        }
    }
}