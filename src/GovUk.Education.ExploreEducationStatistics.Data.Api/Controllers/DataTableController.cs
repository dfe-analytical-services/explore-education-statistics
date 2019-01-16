using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataTableController : ControllerBase
    {
        private readonly DataService _dataService;

        [HttpGet]
        public ActionResult<List<Models.Data>> Get()
        {
            return new List<Models.Data>
            {
                new Models.Data {Domain = "2012/13", Range = Range("4.2", "1.1", "5.3")},
                new Models.Data {Domain = "2013/14", Range = Range("3.5", "1.1", "4.6")},
                new Models.Data {Domain = "2014/15", Range = Range("3.5", "1.1", "4.6")},
                new Models.Data {Domain = "2015/16", Range = Range("1.1", "3.4", "4.5")},
                new Models.Data {Domain = "2016/17", Range = Range("1.3", "3.4", "4.7")}
            };
        }

        private Dictionary<string, string> Range(string authorised, string unauthorised, string overall)
        {
            var range = new Dictionary<string, string>();

            range.Add("authorised", authorised);
            range.Add("unauthorised", unauthorised);
            range.Add("overall", overall);

            return range;
        }
    }
}