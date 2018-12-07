using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<GeographicModel>> List(int releaseId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{countryId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int countryId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new GeographicModel(201617, "National", new Country("E92000001", "England"), null, null, null, SchoolType.Total);
        }

        [HttpGet("{countryId}/regions")]
        public ActionResult<List<GeographicModel>> GetRegions(int releaseId, int countryId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{countryId}/local-authorities")]
        public ActionResult<List<GeographicModel>> GetLocalAuthorities(int releaseId, int countryId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{countryId}/schools")]
        public ActionResult<List<GeographicModel>> getSchools(int releaseId, int countryId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();

        }
    }
}