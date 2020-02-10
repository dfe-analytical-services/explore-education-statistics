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
    public class DownloadController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public DownloadController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>> GetDownloadTree()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentDownloadTreePath()), NoContent());
        }
    }
}