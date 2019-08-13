using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly IPublicationService _publicationService;
        private readonly IReleaseService _releaseService;

        private readonly IContentCacheService _contentCacheService;

        public ContentController(
            IContentService contentService,
            IPublicationService publicationService,
            IReleaseService releaseService,
            IContentCacheService contentCacheService
            )
        {
            _contentService = contentService;
            _publicationService = publicationService;
            _releaseService = releaseService;

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
        public ActionResult<PublicationViewModel> GetPublication(string slug)
        {
            var publication = _publicationService.GetPublication(slug);

            if (publication != null)
            {
                return publication;
            }

            return NotFound();
        }

        // GET api/content/publication/pupil-absence-in-schools-in-england/latest
        [HttpGet("publication/{publicationSlug}/latest")]
        public ActionResult<ReleaseViewModel> GetLatestRelease(string publicationSlug)
        {
            var release = _releaseService.GetLatestRelease(publicationSlug);

            if (release != null)
            {
                return release;
            }

            return NotFound();
        }

        // GET api/content/release/5
        [HttpGet("release/{id}")]
        public ActionResult<Release> GetRelease(string id)
        {
            var release = _releaseService.GetRelease(id);

            if (release != null)
            {
                return release;
            }

            return NotFound();
        }
    }
}