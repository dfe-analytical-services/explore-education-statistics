using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IAttributeMetaService _attributeMetaService;
        private readonly ICharacteristicMetaService _characteristicMetaService;
        private readonly IMapper _mapper;

        public TableBuilderController(ITableBuilderService tableBuilderService,
            IAttributeMetaService attributeMetaService,
            ICharacteristicMetaService characteristicMetaService)
        {
            _tableBuilderService = tableBuilderService;
            _attributeMetaService = attributeMetaService;
            _characteristicMetaService = characteristicMetaService;
        }
        
        public TableBuilderController(ITableBuilderService tableBuilderService,
            IAttributeMetaService attributeMetaService,
            ICharacteristicMetaService characteristicMetaService,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _attributeMetaService = attributeMetaService;
            _characteristicMetaService = characteristicMetaService;
            _mapper = mapper;
        }

        [HttpGet("geographic/{publicationId}/{level}")]
        public ActionResult<TableBuilderResult> GetGeographic(Guid publicationId,
            [CommaSeparatedQueryString] ICollection<int> years,
            [FromQuery(Name = "startYear")] int startYear,
            [FromQuery(Name = "endYear")] int endYear,
            [CommaSeparatedQueryString] ICollection<SchoolType> schoolTypes,
            [CommaSeparatedQueryString] ICollection<string> attributes,
            Level level = Level.National)
        {
            var query = new GeographicQueryContext
            {
                Attributes = attributes,
                Level = level,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years,
                StartYear = startYear,
                EndYear = endYear
            };

            return _tableBuilderService.GetGeographic(query);
        }

        [HttpPost("geographic")]
        public ActionResult<TableBuilderResult> GetGeographic([FromBody] GeographicQueryContext query)
        {
            var result =_tableBuilderService.GetGeographic(query);
            if (!result.Result.Any())
            {
                return new NotFoundResult();
            }
            
            return result;
        }

        [HttpGet("characteristics/local-authority/{publicationId}")]
        public ActionResult<TableBuilderResult> GetLocalAuthority(Guid publicationId,
            [CommaSeparatedQueryString] ICollection<int> years,
            [FromQuery(Name = "startYear")] int startYear,
            [FromQuery(Name = "endYear")] int endYear,
            [CommaSeparatedQueryString] ICollection<SchoolType> schoolTypes,
            [CommaSeparatedQueryString] ICollection<string> attributes,
            [CommaSeparatedQueryString] ICollection<string> characteristics)
        {
            var query = new LaQueryContext
            {
                Attributes = attributes,
                Characteristics = characteristics,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years,
                StartYear = startYear,
                EndYear = endYear
            };

            return _tableBuilderService.GetLocalAuthority(query);
        }

        [HttpPost("characteristics/local-authority")]
        public ActionResult<TableBuilderResult> GetLocalAuthority([FromBody] LaQueryContext query)
        {
            var result = _tableBuilderService.GetLocalAuthority(query);
            if (!result.Result.Any())
            {
                return new NotFoundResult();
            }
            
            return result;
        }

        [HttpGet("characteristics/national/{publicationId}")]
        public ActionResult<TableBuilderResult> GetNational(Guid publicationId,
            [CommaSeparatedQueryString] ICollection<int> years,
            [FromQuery(Name = "startYear")] int startYear,
            [FromQuery(Name = "endYear")] int endYear,
            [CommaSeparatedQueryString] ICollection<SchoolType> schoolTypes,
            [CommaSeparatedQueryString] ICollection<string> attributes,
            [CommaSeparatedQueryString] ICollection<string> characteristics)
        {
            var query = new NationalQueryContext
            {
                Attributes = attributes,
                Characteristics = characteristics,
                PublicationId = publicationId,
                SchoolTypes = schoolTypes,
                Years = years,
                StartYear = startYear,
                EndYear = endYear
            };

            return _tableBuilderService.GetNational(query);
        }

        [HttpPost("characteristics/national")]
        public ActionResult<TableBuilderResult> GetNational(NationalQueryContext query)
        {
            var result = _tableBuilderService.GetNational(query);
            if (!result.Result.Any())
            {
                return new NotFoundResult();
            }
            
            return result;
        }

        [HttpGet("meta/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetMeta(Guid publicationId)
        {
            return new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Attributes = _attributeMetaService.Get(publicationId)
                    .GroupBy(o => o.Group)
                    .ToDictionary(
                        metas => metas.Key,
                        metas => metas.Select(ToAttributeMetaViewModel).ToList()),
                Characteristics = _characteristicMetaService.Get(publicationId)
                    .GroupBy(o => o.Group)
                    .ToDictionary(
                        metas => metas.Key,
                        metas => metas.Select(ToNameLabelViewModel).ToList())
            };
        }

        private AttributeMetaViewModel ToAttributeMetaViewModel(AttributeMeta attributeMeta)
        {
            return _mapper.Map<AttributeMetaViewModel>(attributeMeta);
        }

        private NameLabelViewModel ToNameLabelViewModel(CharacteristicMeta characteristicMeta)
        {
            return _mapper.Map<NameLabelViewModel>(characteristicMeta);
        }
    }
}