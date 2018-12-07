using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        [HttpGet("{publicationId}")]
        public ActionResult<List<GeographicModel>> Get(int publicationId)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}