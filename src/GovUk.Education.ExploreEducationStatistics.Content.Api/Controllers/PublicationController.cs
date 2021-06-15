using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PublicationController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public PublicationController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("publications/{slug}/title")]
        public async Task<ActionResult<PublicationTitleViewModel>> GetPublicationTitle(string slug)
        {
            return await _fileStorageService
                .GetDeserialized<PublicationTitleViewModel>(PublicContentPublicationPath(slug))
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{slug}/methodology")]
        public async Task<ActionResult<PublicationMethodologyViewModel>> GetPublicationMethodology(string slug)
        {
            return await _fileStorageService
                .GetDeserialized<PublicationMethodologyViewModel>(PublicContentPublicationPath(slug))
                .HandleFailuresOrOk();
        }
    }
}