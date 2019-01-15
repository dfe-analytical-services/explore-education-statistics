using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("data/{publication}/geo-levels/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICsvReader _csvReader;
        private readonly DataService _dataService;
        private readonly IMapper _mapper;


        public CountryController(ICsvReader csvReader, DataService dataService, IMapper mapper)
        {
            _csvReader = csvReader;
            _dataService = dataService;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<CountryGeographicModel>> List(string publication,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            var data = _dataService.Get(publication, "national").Where(x =>
                (string.IsNullOrEmpty(schoolType) ||
                 string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                (!year.HasValue || x.Year == year)
            ).ToList();


            var model = new List<CountryGeographicModel>();

            foreach (var c in data.GroupBy(d => d.Country.Code))
            {
                var g = new CountryGeographicModel();
                g.Years = new List<YearItem>();

                foreach (var y in c.Where(x => x.SchoolType == "Total"))
                {
                    g.Years.Add(new YearItem {Year = y.Year, Attributes = y.Attributes});
                }

                g.Level = c.FirstOrDefault().Level;
                g.SchoolType = c.FirstOrDefault().SchoolType;

                model.Add(g);
            }

            return model;
        }

        [HttpGet("{countryId}")]
        public ActionResult<CountryGeographicModel> Get(string publication, string countryId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            var data = _dataService.Get(publication, "national").Where(x =>
                (string.IsNullOrEmpty(schoolType) ||
                 string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                (!year.HasValue || x.Year == year)
            ).ToList();


            var model = new CountryGeographicModel();

            model.Years = new List<YearItem>();

            foreach (var y in data.Where(x => x.SchoolType == "Total"))
            {
                model.Years.Add(new YearItem {Year = y.Year, Attributes = y.Attributes});
            }

            model.Level = data.FirstOrDefault().Level;
            model.SchoolType = data.FirstOrDefault().SchoolType;

            return model;
        }

        [HttpGet("{countryId}/characteristics")]
        public ActionResult<List<NationalCharacteristicModel>> GetCharacteristics(string publication, string countryId,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.NationalCharacteristics(publication + "_natcharacteristics", attributes)
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
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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