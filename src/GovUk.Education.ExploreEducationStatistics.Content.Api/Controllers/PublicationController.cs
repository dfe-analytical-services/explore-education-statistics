using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/content")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PublicationController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public PublicationController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationTreeNode>>>> GetPublicationTree()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<PublicationTreeNode>>>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationsTreePath()), NoContent());
        }

        [HttpGet("publication/{slug}/title")]
        public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
        {
            return await this.JsonContentResultAsync<PublicationTitleViewModel>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationPath(slug)), NotFound());
        }
    }
}