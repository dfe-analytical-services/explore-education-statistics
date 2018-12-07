using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data/{publicationId}/{releaseId}/geo-levels/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<string> List(int publicationId, int releaseId)
        {
            return "value";
        }

        [HttpGet("{countryId}")]
        public ActionResult<string> Get(int publicationId, int releaseId, int countryId)
        {
            return "value";
        }

        [HttpGet("{countryId}/regions")]
        public ActionResult<string> GetRegions(int publicationId, int releaseId, int countryId)
        {
            return "value";
        }

        [HttpGet("{countryId}/local-authorities")]
        public ActionResult<string> GetLocalAuthorities(int publicationId, int releaseId, int countryId)
        {
            return "value";
        }

        [HttpGet("{countryId}/schools")]
        public ActionResult<string> getSchools(int publicationId, int releaseId, int countryId)
        {
            return "value";
        }
    }
}