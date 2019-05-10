using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        [HttpGet("publication/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetPublicationMeta(Guid publicationId)
        {
            return _metaService.GetPublicationMeta(publicationId);
        }

        [HttpGet("subject/{subjectId}")]
        public ActionResult<SubjectMetaViewModel> GetSubjectMeta(long subjectId)
        {
            return _metaService.GetSubjectMeta(subjectId);
        }
        
        [HttpPost("subject")]
        public ActionResult<SubjectMetaViewModel> GetSubjectMeta([FromBody] SubjectMetaQueryContext query)
        {
            return _metaService.GetSubjectMeta(query);
        }
    }
}