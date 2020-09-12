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
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class DownloadController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public DownloadController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>> GetDownloadTree()
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
    }
}