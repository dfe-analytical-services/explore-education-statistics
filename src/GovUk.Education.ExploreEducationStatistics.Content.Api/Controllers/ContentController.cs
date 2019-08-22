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
        public async Task<ActionResult<List<ThemeTree>>> GetContentTree()
        {
            var tree = await _contentCacheService.GetContentTreeAsync();

            if (tree.Any())
            {
                return tree;
            }

            return NoContent();
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england
        [HttpGet("publication/{slug}")]
        public async Task<ActionResult<PublicationViewModel>> GetPublication(string slug)
        {
            var publication = await _contentCacheService.GetPublicationAsync(slug);

            if (publication != null)
            {
                return publication;
            }

            return NotFound();
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england/latest
        [HttpGet("publication/{publicationSlug}/latest")]
        public async Task<ActionResult<ReleaseViewModel>> GetLatestRelease(string publicationSlug)
        {
            var release = await _contentCacheService.GetLatestReleaseAsync(publicationSlug);

            if (release != null)
            {
                return release;
            }

            return NotFound();
        }

        // TODO: this looks like it needs refactoring to return the release view model
        // GET api/content/release/5
        [HttpGet("release/{id}")]
        public async Task<ActionResult<Release>> GetRelease(string id)
        {
            var release = await _contentCacheService.GetReleaseAsync(id);

            if (release != null)
            {
                return release;
            }

            return NotFound();
        }
    }
}