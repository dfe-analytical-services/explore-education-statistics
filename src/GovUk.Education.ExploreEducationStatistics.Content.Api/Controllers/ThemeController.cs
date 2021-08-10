#nullable enable
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IPublicationService _publicationService;
        private readonly IMethodologyService _methodologyService;

        public ThemeController(
            IPublicationService publicationService,
            IMethodologyService methodologyService)
        {
            _publicationService = publicationService;
            _methodologyService = methodologyService;
        }

        [HttpGet("themes")]
        public async Task<IList<ThemeTree<PublicationTreeNode>>> GetThemes()
        {
            return await _publicationService.GetPublicationTree();
        }

        [HttpGet("download-themes")]
        public async Task<IList<ThemeTree<PublicationDownloadsTreeNode>>> GetDownloadThemes()
        {
            return await _publicationService.GetPublicationDownloadsTree();
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<List<AllMethodologiesThemeViewModel>>> GetMethodologyThemes()
        {
            return await _methodologyService.GetTree()
                .HandleFailuresOrOk();
        }
    }
}
