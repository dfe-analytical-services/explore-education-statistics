using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly IPublicationMetaService _publicationMetaService;
        private readonly IThemeMetaService _themeMetaService;

        public MetaController(IPublicationMetaService publicationMetaService, IThemeMetaService themeMetaService)
        {
            _publicationMetaService = publicationMetaService;
            _themeMetaService = themeMetaService;
        }

        [HttpGet("themes")]
        public ActionResult<IEnumerable<ThemeMetaViewModel>> GetThemes()
        {
            return _themeMetaService.GetThemes().ToList();
        }

        [HttpGet("publication/{publicationId}")]
        public ActionResult<PublicationSubjectsMetaViewModel> GetSubjectsForLatestRelease(Guid publicationId)
        {
            var viewModel = _publicationMetaService.GetSubjectsForLatestRelease(publicationId);
            if (viewModel == null)
            {
                return NotFound();
            }

            return viewModel;
        }
    }
}