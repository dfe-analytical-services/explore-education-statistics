using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Themes once the current Crud theme controller is removed
    [Authorize]
    [ApiController]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        // GET api/me/themes/
        [HttpGet("api/me/themes")]
        public async Task<ActionResult<List<Theme>>> GetMyThemes()
        {
            return await _themeService
                .GetMyThemesAsync()
                .HandleFailuresOr(Ok);
        }

        // GET api/theme/{themeId}/summary
        [HttpGet("api/theme/{themeId}/summary")]
        public async Task<ActionResult<TitleAndIdViewModel>> GetThemeById([Required] Guid themeId)
        {
            return await _themeService
                .GetSummaryAsync(themeId)
                .HandleFailuresOr(Ok);
        }
    }
}