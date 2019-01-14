using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("data/{publication}/geo-levels/local-authority")]
    [ApiController]
    public class LocalAuthorityController : ControllerBase
    {
        private readonly ICsvReader _csvReader;

        public LocalAuthorityController(ICsvReader csvReader)
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
                    x.Level.ToLower() == "local authority"
                ).ToList();
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