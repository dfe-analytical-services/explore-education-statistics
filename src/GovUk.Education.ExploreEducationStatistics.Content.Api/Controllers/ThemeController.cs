#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;
        private readonly IMethodologyService _methodologyService;

        public ThemeController(
            IThemeService themeService,
            IMethodologyService methodologyService)
        {
            _themeService = themeService;
            _methodologyService = methodologyService;
        }

        [HttpGet("themes")]
        public async Task<IList<ThemeTree<PublicationTreeNode>>> GetThemes()
        {
            return await _themeService.GetPublicationTree();
        }

        [HttpGet("download-themes")]
        public async Task<IList<ThemeTree<PublicationDownloadsTreeNode>>> GetDownloadThemes()
        {
            return await _themeService.GetPublicationDownloadsTree();
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
        {
            return await _methodologyService.GetTree()
                .HandleFailuresOrOk();
        }
    }
}
