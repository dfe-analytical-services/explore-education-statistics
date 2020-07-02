using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Themes once the current Crud theme controller is removed
    [Route("api")]
    [Authorize]
    [ApiController]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        // POST api/theme/
        [HttpPost("theme")]
        public async Task<ActionResult<ThemeViewModel>> CreateTheme(CreateThemeRequest request)
        {
            return await _themeService
                .CreateTheme(request)
                .HandleFailuresOrOk();
        }

        // GET api/me/themes/
        [HttpGet("me/themes")]
        public async Task<ActionResult<List<Theme>>> GetMyThemes()
        {
            return await _themeService
                .GetMyThemes()
                .HandleFailuresOrOk();
        }

        // GET api/theme/{themeId}/summary
        [HttpGet("theme/{themeId}/summary")]
        public async Task<ActionResult<TitleAndIdViewModel>> GetThemeById([Required] Guid themeId)
        {
            return await _themeService
                .GetSummary(themeId)
                .HandleFailuresOrOk();
        }
    }
}