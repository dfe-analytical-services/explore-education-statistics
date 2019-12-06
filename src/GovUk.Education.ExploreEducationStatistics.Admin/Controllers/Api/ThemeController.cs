using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
        public ActionResult<List<Theme>> GetMyThemes()
        {
            if (true)
            {
                return new ForbidResult();
            }

            var userId = new Guid(); // TODO get the Guid from AD

            var result = _themeService.GetUserThemes(userId);

            if (result.Any())
            {
                return result;
            }

            return NoContent();
        }

        // GET api/theme/{themeId}/summary
        [HttpGet("api/theme/{themeId}/summary")]
        public async Task<ActionResult<ThemeSummaryViewModel>> GetThemeById([Required] Guid themeId)
        {
            var theme = await _themeService.GetSummaryAsync(themeId);

            if (theme == null)
            {
                return NotFound();
            }

            return theme;
        }
    }
}