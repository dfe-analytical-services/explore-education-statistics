using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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

        // GET api/content/tree
        /// <response code="204">If the item is null</response>    
        [HttpGet("tree")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(List<ThemeTree>), 200)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetPublicationsTree()
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationsTreePath()), NoContent());
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england
        [HttpGet("publication/{slug}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(PublicationViewModel), 200)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetPublication(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentPublicationPath(slug)), NotFound());
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england/latest
        [HttpGet("publication/{publicationSlug}/latest")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ReleaseViewModel), 200)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetLatestRelease(string slug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentLatestReleasePath(slug)), NotFound());
        }

        // TODO: this looks like it needs refactoring to return the release view model
        // GET api/content/publication/pupil-absence-in-schools-in-england/2017-18
        [HttpGet("publication/{publicationSlug}/{releaseSlug}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ReleaseViewModel), 200)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetRelease(string publicationSlug, string releaseSlug)
        {
            return await this.JsonContentResultAsync(() =>
                _fileStorageService.DownloadTextAsync(PublicContentReleasePath(publicationSlug, releaseSlug)), NotFound());
        }
    }
}