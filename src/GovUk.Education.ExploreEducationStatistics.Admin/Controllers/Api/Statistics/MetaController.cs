using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
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