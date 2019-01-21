using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly TableBuilderService _tableBuilderService;

        public TableBuilderController(TableBuilderService tableBuilderService)
        {
            _tableBuilderService = tableBuilderService;
        }

        [HttpGet("geographic/{publicationId}/{schoolType}/{level}")]
        public ActionResult<TableBuilderResult> GetGeographic(Guid publicationId,
            SchoolType schoolType,
            [FromQuery(Name = "years")] ICollection<int> years,
            [FromQuery(Name = "attributes")] ICollection<string> attributes,
            Level level = Level.National)
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
        
        [HttpGet("local-authority/{publicationId}/{schoolType}/{level}")]
        public ActionResult<TableBuilderResult> GetLocalAuthority(Guid publicationId,
            SchoolType schoolType,
            [FromQuery(Name = "years")] ICollection<int> years,
            [FromQuery(Name = "attributes")] ICollection<string> attributes,
            Level level = Level.National)
        {
            var query = new LaQueryContext
            {
                Attributes = attributes,
                Level = level,
                PublicationId = publicationId,
                SchoolType = schoolType,
                Years = years
            };
            
            return _tableBuilderService.GetLocalAuthority(query);
        }
        
        [HttpGet("national/{publicationId}/{schoolType}/{level}")]
        public ActionResult<TableBuilderResult> GetNational(Guid publicationId,
            SchoolType schoolType,
            [FromQuery(Name = "years")] ICollection<int> years,
            [FromQuery(Name = "attributes")] ICollection<string> attributes,
            Level level = Level.National)
        {
            var query = new NationalQueryContext
            {
                Attributes = attributes,
                Level = level,
                PublicationId = publicationId,
                SchoolType = schoolType,
                Years = years
            };
            
            return _tableBuilderService.GetNational(query);
        }
    }
}