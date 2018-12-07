using System.Collections.Generic;
using System.Linq;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("data")]
    [ApiController]
    public class ReleaseController : ControllerBase
    {
        [HttpGet("{releaseId}")]
        public ActionResult<List<GeographicModel>> Get(int releaseId)
        {
            return Enumerable.Empty<GeographicModel>().ToList();
        }
    }
}