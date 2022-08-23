#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;
        private readonly IContentCacheService _contentCacheService;

        public ThemeController(
            IThemeService themeService,
            IContentCacheService contentCacheService)
        {
            _themeService = themeService;
            _contentCacheService = contentCacheService;
        }

        [HttpGet("themes")]
        public async Task<ActionResult<IList<ThemeTree<PublicationTreeNode>>>> GetPublicationTree(
            [FromQuery(Name = "publicationFilter")] PublicationTreeFilter? filter = null)
        {
            if (filter == null)
            {
                return new BadRequestResult();
            }
            return await _themeService.GetPublicationTree(filter.Value)
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
        {
            return await _contentCacheService
                .GetMethodologyTree()
                .HandleFailuresOrOk();
        }
    }
}
