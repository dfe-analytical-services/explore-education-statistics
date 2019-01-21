using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.DataTable;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
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

        [HttpGet("geographic/{publicationId}/{schoolType}/{level}")]
        public ActionResult<TableToolResult> GetGeographic(Guid publicationId,
            SchoolType schoolType,
            [FromQuery(Name = "years")] ICollection<int> years,
            [FromQuery(Name = "attributes")] ICollection<string> attributes,
            Level level = Level.national)
        {
            var query = new GeographicQueryContext
            {
                Attributes = attributes,
                Level = level,
                PublicationId = publicationId,
                SchoolType = schoolType,
                Years = years
            };
            
            return _tableBuilderService.GetGeographic(query);
        }
    }
}