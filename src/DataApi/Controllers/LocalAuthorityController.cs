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
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }

        [HttpGet("{localAuthorityId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new GeographicModel
            {
                Year = 201617,
                Level = "National",
                Country = new Country
                {
                    Code = "E92000001",
                    Name = "England"
                },
                Region = new Region
                {
                    Code = "E13000001",
                    Name = "Inner London"
                },
                LocalAuthority = new LocalAuthority
                {
                    Name = "City of London",
                    Code = "E09000001",
                    Old_Code = "201"
                },
                School = new School
                {
                    laestab = 2013614
                },
                SchoolType = "Total",
                Attributes = new Dictionary<string, int>()
            };
        }

        [HttpGet("{localAuthorityId}/schools")]
        public ActionResult<List<GeographicModel>> GetSchools(int releaseId, int localAuthorityId,
            [FromQuery(Name = "schoolType")] string schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}