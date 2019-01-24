using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("data")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private MetaService _metaService;

        public PublicationController(MetaService metaService)
        {
            _metaService = metaService;
        }

        [HttpGet("meta/attributes/{publicationId}")]
        public ActionResult<IEnumerable<AttributeMeta>> GetAttributeMeta(Guid publicationId)
        {
            return _metaService.GetAttributeMeta(publicationId).ToList();
        }

        [HttpGet("meta/characteristics/{publicationId}")]
        public ActionResult<IEnumerable<CharacteristicMeta>> GetCharacteristicMeta(Guid publicationId)
        {
            return _metaService.GetCharacteristicMeta(publicationId).ToList();
        }
    }
}