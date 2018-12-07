using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/region")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<GeographicModel>> List(int releaseId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{regionId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int regionId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new GeographicModel(201617, Level.National, new Country("E92000001", "England"), new Region("E12000001", "North East"), null, null, SchoolType.Total);
        }

        [HttpGet("{regionId}/local-authorities")]
        public ActionResult<List<GeographicModel>> GetLocalAuthorities(int releaseId, int regionId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{regionId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(int releaseId, int regionId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}