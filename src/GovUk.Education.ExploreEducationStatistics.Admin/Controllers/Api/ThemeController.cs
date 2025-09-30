using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[Authorize]
[ApiController]
public class ThemeController(IThemeService themeService) : ControllerBase
{
    [HttpPost("themes")]
    public async Task<ActionResult<ThemeViewModel>> CreateTheme(ThemeSaveViewModel theme)
    {
        return await themeService.CreateTheme(theme).HandleFailuresOrOk();
    }

    [HttpPut("themes/{themeId:guid}")]
    public async Task<ActionResult<ThemeViewModel>> UpdateTheme(
        [Required] Guid themeId,
        ThemeSaveViewModel theme
    )
    {
        return await themeService.UpdateTheme(themeId, theme).HandleFailuresOrOk();
    }

    [HttpGet("themes/{themeId:guid}")]
    public async Task<ActionResult<ThemeViewModel>> GetTheme([Required] Guid themeId)
    {
        return await themeService.GetTheme(themeId).HandleFailuresOrOk();
    }

    [HttpGet("themes")]
    public async Task<ActionResult<List<ThemeViewModel>>> GetThemes()
    {
        return await themeService.GetThemes().HandleFailuresOrOk();
    }

    [HttpDelete("themes/{themeId:guid}")]
    public async Task<ActionResult> DeleteTheme([Required] Guid themeId)
    {
        return await themeService.DeleteTheme(themeId).HandleFailuresOrNoContent();
    }

    [HttpDelete("themes")]
    public async Task<ActionResult> DeleteUITestThemes(CancellationToken cancellationToken)
    {
        return await themeService.DeleteUITestThemes(cancellationToken).HandleFailuresOrNoContent();
    }
}
