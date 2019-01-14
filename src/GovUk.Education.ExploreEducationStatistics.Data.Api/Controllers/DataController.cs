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
        public ActionResult<List<TidyData>> Get()
        {
            return _dataService.Get();
        }

        [HttpGet("{id:length(24)}", Name = "GetBook")]
        public ActionResult<TidyData> Get(string id)
        {
            var tidyData = _dataService.Get(id);

            if (tidyData == null)
            {
                return NotFound();
            }

            return tidyData;
        }

        [HttpPost]
        public ActionResult<TidyData> Create(TidyData book)
        {
            _dataService.Create(book);

            return CreatedAtRoute("GetBook", new { id = book.Id.ToString() }, book);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, TidyData bookIn)
        {
            var tidyData = _dataService.Get(id);

            if (tidyData == null)
            {
                return NotFound();
            }

            _dataService.Update(id, bookIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var tidyData = _dataService.Get(id);

            if (tidyData == null)
            {
                return NotFound();
            }

            _dataService.Remove(tidyData.Id);

            return NoContent();
        }
    }
}