using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("themes")]
        public ActionResult<IEnumerable<ThemeMetaViewModel>> GetThemes()
        {
            return _publicationMetaService.GetThemes().ToList();
        }
        
        [HttpGet("publication/{publicationId}")]
        public ActionResult<PublicationSubjectsMetaViewModel> GetPublication(Guid publicationId)
        {
            var viewModel = _publicationMetaService.GetPublication(publicationId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }
    }
}