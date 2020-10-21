using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ThemeController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public ThemeController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpGet("themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationTreeNode>>>> GetThemes()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<PublicationTreeNode>>>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentPublicationsTreePath()
                    ),
                NoContent()
            );
        }

        [HttpGet("download-themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>> GetDownloadThemes()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentDownloadTreePath()
                    ),
                NoContent()
            );
        }

        [HttpGet("methodology-themes")]
        public async Task<ActionResult<IEnumerable<ThemeTree<MethodologyTreeNode>>>> GetMethodologyThemes()
        {
            return await this.JsonContentResultAsync<IEnumerable<ThemeTree<MethodologyTreeNode>>>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentMethodologyTreePath()
                    ),
                NoContent()
            );
        }
    }
}