using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class MethodologyController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public MethodologyController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("methodologies/{slug}")]
        public async Task<ActionResult<MethodologyViewModel>> Get(string slug)
        {
            return await _fileStorageService.GetDeserialized<MethodologyViewModel>(PublicContentMethodologyPath(slug))
                .HandleFailuresOrOk();
        }
    }
}