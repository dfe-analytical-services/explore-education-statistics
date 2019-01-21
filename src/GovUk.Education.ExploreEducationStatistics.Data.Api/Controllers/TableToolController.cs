using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableToolController : ControllerBase
    {
        private readonly TableBuilderService _tableBuilderService;

        public TableToolController(TableBuilderService tableBuilderService)
        {
            _tableBuilderService = tableBuilderService;
        }

        [HttpGet("{publicationId}/{schoolType}/{level}")]
        public ActionResult<TableToolResult> Get(string publicationId,
            SchoolType schoolType,
            [FromQuery(Name = "years")] ICollection<int> yearFilter,
            [FromQuery(Name = "attributes")] ICollection<string> attributeFilter,
            Level level = Level.national)
        {
            return _tableBuilderService.Get(Guid.Parse(publicationId), schoolType, level, yearFilter, attributeFilter);
        }
    }
}