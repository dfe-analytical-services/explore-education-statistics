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
            return new CsvReader().GeoLevels().Where(x => x.Year == 201617 && string.IsNullOrWhiteSpace(x.LocalAuthority.Code)).ToList();
        }

        [HttpGet("{regionId}")]
        public ActionResult<GeographicModel> Get(int releaseId, int regionId,
            [FromQuery(Name = "school-type")] SchoolType schoolType,
            [FromQuery(Name = "attributes")] List<string> attributes)
        {
            return new GeographicModel
            {
                Year = 201617,
                Level = Level.National,
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
                LocalAuthority = null,
                laestab = null,
                SchoolType = SchoolType.Total,
                Attributes = new Dictionary<string, int>()
            };
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