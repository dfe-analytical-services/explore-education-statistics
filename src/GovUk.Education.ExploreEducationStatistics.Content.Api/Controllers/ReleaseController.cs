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
    public class ReleaseController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public ReleaseController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("publication/{slug}/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string slug)
        {
            return await this.JsonContentResultAsync<ReleaseViewModel>(() =>
                _fileStorageService.DownloadTextAsync(PublicContentLatestReleasePath(slug)), NotFound());
        }

        [HttpGet("publication/{publicationSlug}/{releaseSlug}")]
        public async Task<ActionResult<ReleaseViewModel>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await this.JsonContentResultAsync<ReleaseViewModel>(
                () => _fileStorageService.DownloadTextAsync(PublicContentReleasePath(publicationSlug, releaseSlug)),
                NotFound());
        }
    }
}