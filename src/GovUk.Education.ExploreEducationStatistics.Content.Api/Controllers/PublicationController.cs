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
    [Route("api/content")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PublicationController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public PublicationController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<ThemeTree<PublicationTreeNode>>>> GetPublicationTree()
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

        [HttpGet("publication/{slug}/title")]
        public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
        {
            return await this.JsonContentResultAsync<PublicationTitleViewModel>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentPublicationPath(slug)
                    ),
                NotFound()
            );
        }

        [HttpGet("publication/{slug}/methodology")]
        public async Task<ActionResult<PublicationMethodologyViewModel>> GetPublicationMethodology(string slug)
        {
            return await this.JsonContentResultAsync<PublicationMethodologyViewModel>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentPublicationPath(slug)
                    ),
                NotFound()
            );
        }
    }
}