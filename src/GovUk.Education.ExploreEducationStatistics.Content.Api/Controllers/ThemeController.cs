#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IThemeCacheService _themeCacheService;

        public ThemeController(
            IMethodologyCacheService methodologyCacheService,
            IThemeCacheService themeCacheService)
        {
            _methodologyCacheService = methodologyCacheService;
            _themeCacheService = themeCacheService;
        }

        [HttpGet("themes")]
        public async Task<ActionResult<IList<ThemeTree>>> GetPublicationTree(
            [FromQuery(Name = "publicationFilter")] PublicationTreeFilter? filter = null)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }

            return await _themeCacheService
                .GetPublicationTree(filter.Value)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
        {
            return await _methodologyCacheService
                .GetSummariesTree()
                .HandleFailuresOrOk();
        }
    }
}
