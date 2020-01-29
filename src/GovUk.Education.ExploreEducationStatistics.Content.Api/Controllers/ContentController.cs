using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public ContentController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("tree")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetPublicationsTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationsTreePath()), NoContent());
        }

        [HttpGet("publication/{slug}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetPublication(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationPath(slug)), NotFound());
        }

        [HttpGet("publication/{slug}/latest")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetLatestRelease(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentLatestReleasePath(slug)), NotFound());
        }

        [HttpGet("publication/{publicationSlug}/{releaseSlug}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<string>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await this.JsonContentResultAsync(
                () => _fileStorageService.DownloadTextAsync(PublicContentReleasePath(publicationSlug, releaseSlug)),
                NotFound());
        }
    }
}