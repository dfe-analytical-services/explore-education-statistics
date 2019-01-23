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
    [Route("data/{publication}/geo-levels/school")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ICsvReader _csvReader;
        private readonly DataService _dataService;
        private readonly IMapper _mapper;

        public SchoolController(ICsvReader csvReader, DataService dataService, IMapper mapper)
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
            var data = _dataService.Get(publication, "school").Where(x =>
                (string.IsNullOrEmpty(schoolType) ||
                 string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                (!year.HasValue || x.Year == year)
            ).ToList();


            return _mapper.Map<List<GeographicModel>>(data);
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