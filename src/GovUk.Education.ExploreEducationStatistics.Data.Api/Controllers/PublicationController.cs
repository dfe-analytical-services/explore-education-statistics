using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("data")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly ICsvReader _csvReader;

        public PublicationController(ICsvReader csvReader)
        {
            _csvReader = csvReader;
        }
        
        [HttpGet("{publicationId}")]
        public ActionResult<List<GeographicModel>> Get(int publicationId)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}