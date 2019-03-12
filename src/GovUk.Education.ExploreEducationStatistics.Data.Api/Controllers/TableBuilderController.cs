using System;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
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
        private readonly IAttributeMetaService _attributeMetaService;
        private readonly ICharacteristicMetaService _characteristicMetaService;
        private readonly IMapper _mapper;

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
        public ActionResult<PublicationMetaViewModel> GetMeta(Guid publicationId)
        {
            var result = new PublicationMetaViewModel
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

            if (result.Attributes.Any() && result.Characteristics.Any())
            {
                return result;
            }

            return NotFound();
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