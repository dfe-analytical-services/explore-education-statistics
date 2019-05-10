using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        [HttpGet("publication/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetPublicationMeta(Guid publicationId)
        {
            var viewModel = _metaService.GetPublicationMeta(publicationId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }

        [HttpGet("subject/{subjectId}")]
        public ActionResult<SubjectMetaViewModel> GetSubjectMeta(long subjectId)
        {
            var viewModel = _metaService.GetSubjectMeta(subjectId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }

        [HttpPost("subject/{subjectId}")]
        public ActionResult<SubjectMetaViewModel> GetSubjectMeta(long subjectId,
            [FromBody] SubjectMetaQueryContext query)
        {
            var viewModel = _metaService.GetSubjectMeta(subjectId, query);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }
    }
}