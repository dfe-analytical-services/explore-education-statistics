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
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class MethodologyController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public MethodologyController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<ThemeTree<MethodologyTreeNode>>>> GetMethodologyTree()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<MethodologyTreeNode>>>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyTreePath()), NoContent());
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<MethodologyViewModel>> Get(string slug)
        {
            return await this.JsonContentResultAsync<MethodologyViewModel>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentMethodologyPath(slug)), NotFound());
        }
    }
}