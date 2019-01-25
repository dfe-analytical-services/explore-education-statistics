using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly TableBuilderService _tableBuilderService;
        private readonly MetaService _metaService;

        public TableBuilderController(TableBuilderService tableBuilderService, MetaService metaService)
        {
            _tableBuilderService = tableBuilderService;
            _metaService = metaService;
        }

        [HttpGet("geographic/{publicationId}/{level}")]
        public ActionResult<TableBuilderResult> GetGeographic(Guid publicationId,
            [FromQuery(Name = "year")] ICollection<int> years,
            [FromQuery(Name = "schoolType")] ICollection<SchoolType> schoolTypes,
            [FromQuery(Name = "attribute")] ICollection<string> attributes,
            Level level = Level.National)
        {
            var query = new GeographicQueryContext
            {
                Attributes = attributes,
                Level = level,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years
            };

            return _tableBuilderService.GetGeographic(query);
        }

        [HttpGet("characteristics/local-authority/{publicationId}")]
        public ActionResult<TableBuilderResult> GetLocalAuthority(Guid publicationId,
            [FromQuery(Name = "year")] ICollection<int> years,
            [FromQuery(Name = "schoolType")] ICollection<SchoolType> schoolTypes,
            [FromQuery(Name = "attribute")] ICollection<string> attributes,
            [FromQuery(Name = "characteristic")] ICollection<string> characteristics)
        {
            var query = new LaQueryContext
            {
                Attributes = attributes,
                Characteristics = characteristics,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years
            };

            return _tableBuilderService.GetLocalAuthority(query);
        }

        [HttpGet("characteristics/national/{publicationId}")]
        public ActionResult<TableBuilderResult> GetNational(Guid publicationId,
            [FromQuery(Name = "year")] ICollection<int> years,
            [FromQuery(Name = "schoolType")] ICollection<SchoolType> schoolTypes,
            [FromQuery(Name = "attribute")] ICollection<string> attributes,
            [FromQuery(Name = "characteristic")] ICollection<string> characteristics)
        {
            var query = new NationalQueryContext
            {
                Attributes = attributes,
                Characteristics = characteristics,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years
            };

            return _tableBuilderService.GetNational(query);
        }
        
        [HttpGet("meta/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetMeta(Guid publicationId)
        {
            return new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Attributes = _metaService.GetAttributeMeta(publicationId)
                    .GroupBy(o => o.Group)
                    .ToDictionary(
                        metas => metas.Key,
                        metas => metas.Select(ToNameLabelViewModel).ToList()),
                Characteristics = _metaService.GetCharacteristicMeta(publicationId)
                    .GroupBy(o => o.Group)
                    .ToDictionary(
                        metas => metas.Key,
                        metas => metas.Select(ToNameLabelViewModel).ToList())
            };
        }

        private static NameLabelViewModel ToNameLabelViewModel(IMeta characteristicMeta)
        {
            return new NameLabelViewModel
            {
                Name = characteristicMeta.Name,
                Label = characteristicMeta.Label
            };
        }
    }
}