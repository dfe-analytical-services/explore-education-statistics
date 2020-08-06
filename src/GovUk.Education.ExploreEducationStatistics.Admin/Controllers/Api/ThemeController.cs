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

        [HttpPost("theme")]
        public async Task<ActionResult<ThemeViewModel>> CreateTheme(SaveThemeViewModel theme)
        {
            return await _themeService
                .CreateTheme(theme)
                .HandleFailuresOrOk();
        }

        [HttpPut("theme/{themeId}")]
        public async Task<ActionResult<ThemeViewModel>> UpdateTheme(
            [Required] Guid themeId,
            SaveThemeViewModel theme)
        {
            return await _themeService
                .UpdateTheme(themeId, theme)
                .HandleFailuresOrOk();
        }


        [HttpGet("theme/{themeId}")]
        public async Task<ActionResult<ThemeViewModel>> GetTheme([Required] Guid themeId)
        {
            return await _themeService
                .GetTheme(themeId)
                .HandleFailuresOrOk();
        }

        [HttpGet("me/themes")]
        public async Task<ActionResult<List<Theme>>> GetMyThemes()
        {
            return await _themeService
                .GetMyThemes()
                .HandleFailuresOrOk();
        }
    }
}