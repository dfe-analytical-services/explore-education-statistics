using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    // TODO rename to Themes once the current Crud theme controller is removed
    [Authorize]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        // GET api/me/themes/
        [HttpGet("api/me/themes")]
        [AllowAnonymous] // TODO revisit when authentication and authorisation is in place
        public ActionResult<List<Theme>> GetMyThemes()
        {
            var userId = new Guid(); // TODO get the Guid from AD

            var result = _themeService.GetUserThemes(userId);

            if (result.Any())
            {
                return result;
            }

            return NoContent();
        }
    }
}