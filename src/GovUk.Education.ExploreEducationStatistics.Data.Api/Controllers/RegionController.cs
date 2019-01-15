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
    [Route("data/{publication}/geo-levels/region")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly ICsvReader _csvReader;
        private readonly DataService _dataService;
        private readonly IMapper _mapper;

        public RegionController(ICsvReader csvReader, DataService dataService, IMapper mapper)
        {
            _csvReader = csvReader;
            _dataService = dataService;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<GeographicModel>> List(string publication,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            var data = _dataService.Get(publication, "regional").Where(x =>
                (string.IsNullOrEmpty(schoolType) ||
                 string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                (!year.HasValue || x.Year == year)
            ).ToList();


            return _mapper.Map<List<GeographicModel>>(data);
        }

        [HttpGet("{regionId}")]
        public ActionResult<GeographicModel> Get(string publication, string regionId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
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