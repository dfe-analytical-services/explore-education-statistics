using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly IPublicationMetaService _publicationMetaService;

        public MetaController(IPublicationMetaService publicationMetaService)
        {
            _publicationMetaService = publicationMetaService;
        }

        [HttpGet("publication/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetPublicationMeta(Guid publicationId)
        {
            var viewModel = _publicationMetaService.GetPublicationMeta(publicationId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }
    }
}