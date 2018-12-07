using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{releaseId}/geo-levels/local-authority")]
    [ApiController]
    public class LocalAuthorityController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<GeographicModel>> List(int releaseId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{localAuthorityId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new GeographicModel(201617, "National", new Country("E92000001", "England"), new Region("E13000001", "Inner London"), new LocalAuthority("City of London", "E09000001", "201"), null, SchoolType.Total);
        }

        [HttpGet("{localAuthorityId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}