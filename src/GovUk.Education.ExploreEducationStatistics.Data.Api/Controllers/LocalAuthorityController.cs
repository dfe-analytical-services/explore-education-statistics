using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer.Old;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("data/{publication}/geo-levels/local-authority")]
    [ApiController]
    public class LocalAuthorityController : ControllerBase
    {
        private readonly ICsvReader _csvReader;
        private readonly DataService _dataService;
        private readonly IMapper _mapper;

        public LocalAuthorityController(ICsvReader csvReader, DataService dataService, IMapper mapper)
        {
            _csvReader = csvReader;
            _dataService = dataService;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<List<GeographicModel>> List(string publication,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            var data = _dataService.Get(publication, Level.Local_Authority).Where(x => schoolType == x.SchoolType &&
                (!year.HasValue || x.Year == year)
            ).ToList();


            return _mapper.Map<List<GeographicModel>>(data);
        }

        [HttpGet("{localAuthorityId}")]
        public ActionResult<GeographicModel> Get(string publication, string localAuthorityId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
                .FirstOrDefault(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    (x.LocalAuthority.Code == localAuthorityId || x.LocalAuthority.Old_Code == localAuthorityId)
                );
        }

        [HttpGet("{localAuthorityId}/characteristics")]
        public ActionResult<List<LaCharacteristicModel>> GetCharacteristics(string publication, string localAuthorityId,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.LaCharacteristics(publication + "_lacharacteristics", attributes)
                .Where(x =>
                    x.LocalAuthority.Code == localAuthorityId || x.LocalAuthority.Old_Code == localAuthorityId)
                .ToList();
        }

        [HttpGet("{localAuthorityId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(string publication, string localAuthorityId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "year")] int? year,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return _csvReader.GeoLevels(publication + "_geoglevels", attributes)
                .Where(x =>
                    (string.IsNullOrEmpty(schoolType) ||
                     string.Equals(x.SchoolType, schoolType, StringComparison.OrdinalIgnoreCase)) &&
                    (!year.HasValue || x.Year == year) &&
                    (x.LocalAuthority.Code == localAuthorityId || x.LocalAuthority.Old_Code == localAuthorityId) &&
                    x.Level.ToLower() == "school"
                ).ToList();
        }
    }
}