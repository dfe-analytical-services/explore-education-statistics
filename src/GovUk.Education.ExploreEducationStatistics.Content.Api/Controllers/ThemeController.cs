using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public ThemeController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationTreeNode>>>> GetThemes()
        {
            return await _fileStorageService
                .GetDeserialized<IEnumerable<ThemeTree<PublicationTreeNode>>>(PublicContentPublicationsTreePath())
                .HandleFailuresOrOk();
        }

        [HttpGet("download-themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>> GetDownloadThemes()
        {
            return await _fileStorageService
                .GetDeserialized<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>(
                    PublicContentDownloadTreePath()
                )
                .HandleFailuresOrOk();
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<MethodologyTreeNode>>>> GetMethodologyThemes()
        {
            return await _fileStorageService
                .GetDeserialized<IEnumerable<ThemeTree<MethodologyTreeNode>>>(PublicContentMethodologyTreePath())
                .HandleFailuresOrOk();
        }
    }
}