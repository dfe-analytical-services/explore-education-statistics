using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public ActionResult<List<TidyDataGeographic>> Get()
        {
            return _dataService.Get();
        }
        
        [HttpGet("{laEstab}")]
        public ActionResult<TidyDataGeographic> Get(string laEstab)
        {
            return _dataService.Get(laEstab);
        }
    }
}