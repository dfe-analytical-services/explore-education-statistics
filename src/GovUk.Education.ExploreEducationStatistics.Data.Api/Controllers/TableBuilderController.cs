using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly ReleaseService _releaseService;

        public TableBuilderController(ITableBuilderService tableBuilderService, ReleaseService releaseService)
        {
            _tableBuilderService = tableBuilderService;
            _releaseService = releaseService;
        }

        [HttpPost("geographic")]
        public ActionResult<TableBuilderResult> GetGeographic([FromBody] GeographicQueryContext query)
        {
            var result = _tableBuilderService.GetGeographic(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/local-authority")]
        public ActionResult<TableBuilderResult> GetLocalAuthority([FromBody] LaQueryContext query)
        {
            var result = _tableBuilderService.GetLocalAuthority(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/national")]
        public ActionResult<TableBuilderResult> GetNational(NationalQueryContext query)
        {
            var result = _tableBuilderService.GetNational(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpGet("meta/{publicationId}")]
        // TODO Remove me when table builder query changed to include type name
        public ActionResult<PublicationMetaViewModel> GetMeta(Guid publicationId)
        {
            // Table builder currently only works from CharacteristicDataNational data
            return GetMeta(publicationId, typeof(CharacteristicDataNational).Name);
        }

        [HttpGet("meta/{publicationId}/{typeName}")]
        public ActionResult<PublicationMetaViewModel> GetMeta(Guid publicationId, string typeName)
        {
            var type = Type.GetType("GovUk.Education.ExploreEducationStatistics.Data.Api.Models." + typeName);

            var result = new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Attributes = _releaseService.GetAttributeMetas(publicationId, type),
                Characteristics = _releaseService.GetCharacteristicMetas(publicationId, type)
            };

            if (result.Attributes.Any() || result.Characteristics.Any())
            {
                return result;
            }

            return NotFound();
        }
    }
}