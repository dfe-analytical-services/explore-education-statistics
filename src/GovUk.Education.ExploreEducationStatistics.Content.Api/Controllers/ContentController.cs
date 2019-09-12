using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentCacheService _contentCacheService;

        public ContentController(IContentCacheService contentCacheService)
        {
            _contentCacheService = contentCacheService;
        }

        // GET api/content/tree
        /// <response code="204">If the item is null</response>    
        [HttpGet("tree")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetContentTree()
        {
            var tree = await _contentCacheService.GetContentTreeAsync();

            if (string.IsNullOrWhiteSpace(tree))
            {
                return NoContent();
            }

            return tree;
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england
        [HttpGet("publication/{slug}")]
        public async Task<ActionResult<string>> GetPublication(string slug)
        {
            var publication = await _contentCacheService.GetPublicationAsync(slug);

            if (string.IsNullOrWhiteSpace(publication))
            {
                return NotFound();
            }

            return publication;
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england/latest
        [HttpGet("publication/{publicationSlug}/latest")]
        public async Task<ActionResult<string>> GetLatestRelease(string publicationSlug)
        {
            var release = await _contentCacheService.GetLatestReleaseAsync(publicationSlug);

            if (string.IsNullOrWhiteSpace(release))
            {
                return NotFound();
            }

            return release;
        }

        // TODO: this looks like it needs refactoring to return the release view model
        // GET api/content/publication/pupil-absence-in-schools-in-england/2017-18
        [HttpGet("publication/{publicationSlug}/{releaseSlug}")]
        public async Task<ActionResult<string>> GetRelease(string publicationSlug, string releaseSlug)
        {
            var release = await _contentCacheService.GetReleaseAsync(publicationSlug, releaseSlug);

            if (string.IsNullOrWhiteSpace(release))
            {
                return NotFound();
            }

            return release;
        }
    }
}