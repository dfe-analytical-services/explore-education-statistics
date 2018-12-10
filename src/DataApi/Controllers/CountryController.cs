using System;
using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{publication}/geo-levels/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICsvReader _csvReader;

        public CountryController(ICsvReader csvReader)
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
                    x.Level.ToLower() == "national"
                ).ToList();
        }

        [HttpGet("{countryId}")]
        public ActionResult<GeographicModel> Get(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .FirstOrDefault(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Country.Code == countryId &&
                    x.Level.ToLower() == "national"
                );
        }

        [HttpGet("{countryId}/characteristics")]
        public ActionResult<List<NationalCharacteristicModel>> GetCharacteristics(string publication, string countryId)
        {
            return _csvReader.NationalCharacteristics(publication + "_natcharacteristics")
                .Where(x =>
                    x.Country.Code == countryId
                ).ToList();
        }

        [HttpGet("{countryId}/regions")]
        public ActionResult<List<GeographicModel>> GetRegions(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Country.Code == countryId &&
                    x.Level.ToLower() == "region"
                ).ToList();
        }

        [HttpGet("{countryId}/local-authorities")]
        public ActionResult<List<GeographicModel>> GetLocalAuthorities(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Country.Code == countryId &&
                    x.Level.ToLower() == "local authority"
                ).ToList();
        }

        [HttpGet("{countryId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels")
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    x.Country.Code == countryId
                    && x.Level.ToLower() == "school"
                ).ToList();
        }
    }
}