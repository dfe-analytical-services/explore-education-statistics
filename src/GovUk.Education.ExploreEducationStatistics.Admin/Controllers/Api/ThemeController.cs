using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        [HttpPost("themes")]
        public async Task<ActionResult<ThemeViewModel>> CreateTheme(ThemeSaveViewModel theme)
        {
            return await _themeService
                .CreateTheme(theme)
                .HandleFailuresOrOk();
        }

        [HttpPut("themes/{themeId}")]
        public async Task<ActionResult<ThemeViewModel>> UpdateTheme(
            [Required] Guid themeId,
            ThemeSaveViewModel theme)
        {
            return await _themeService
                .UpdateTheme(themeId, theme)
                .HandleFailuresOrOk();
        }


        [HttpGet("themes/{themeId}")]
        public async Task<ActionResult<ThemeViewModel>> GetTheme([Required] Guid themeId)
        {
            return await _themeService
                .GetTheme(themeId)
                .HandleFailuresOrOk();
        }

        [HttpGet("themes")]
        public async Task<ActionResult<List<ThemeViewModel>>> GetThemes()
        {
            return await _themeService
                .GetThemes()
                .HandleFailuresOrOk();
        }

        [HttpDelete("themes/{themeId}")]
        public async Task<ActionResult> DeleteTheme([Required] Guid themeId)
        {
            return await _themeService
                .DeleteTheme(themeId)
                .HandleFailuresOrNoContent();
        }
    }
}
