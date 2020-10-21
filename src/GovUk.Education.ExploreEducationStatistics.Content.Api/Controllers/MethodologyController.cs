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
    public class MethodologyController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public MethodologyController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpGet("methodologies/{slug}")]
        public async Task<ActionResult<MethodologyViewModel>> Get(string slug)
        {
            return await this.JsonContentResultAsync<MethodologyViewModel>(
                () =>
                    _blobStorageService.DownloadBlobText(
                        PublicContentContainerName,
                        PublicContentMethodologyPath(slug)
                    ),
                NotFound()
            );
        }
    }
}