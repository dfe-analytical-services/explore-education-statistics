#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IThemeService _themeService;

        public ThemeController(
            IMethodologyCacheService methodologyCacheService,
            IThemeService themeService)
        {
            _methodologyCacheService = methodologyCacheService;
            _themeService = themeService;
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
        {
            return await _methodologyCacheService
                .GetSummariesTree()
                .HandleFailuresOrOk();
        }

        [MemoryCache(typeof(ListThemesCacheKey), durationInSeconds: 10, expiryScheduleCron: HalfHourlyExpirySchedule)]
        [HttpGet("themes")]
        public async Task<IList<ThemeViewModel>> ListThemes()
        {
            return await _themeService
                .ListThemes();
        }
    }
}
