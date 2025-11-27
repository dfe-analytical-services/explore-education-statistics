#nullable enable
using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
[Produces(MediaTypeNames.Application.Json)]
public class ThemeController(
    IMethodologyCacheService methodologyCacheService,
    IThemeService themeService,
    IMemoryCacheService memoryCacheService,
    ILogger<ThemeController> logger,
    DateTimeProvider dateTimeProvider
) : ControllerBase
{
    [HttpGet("methodology-themes")]
    public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
    {
        return await methodologyCacheService.GetSummariesTree().HandleFailuresOrOk();
    }

    [HttpGet("themes")]
    public Task<IList<ThemeViewModel>> ListThemes()
    {
        return memoryCacheService.GetOrCreateAsync(
            cacheKey: new ListThemesCacheKey(),
            createIfNotExistsFn: themeService.ListThemes,
            durationInSeconds: 10,
            expiryScheduleCron: HalfHourlyExpirySchedule,
            dateTimeProvider: dateTimeProvider,
            logger: logger
        );
    }
}
