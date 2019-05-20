using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : Controller
    {
        private readonly IContentService _contentService;
        private readonly IPublicationService _publicationService;
        private readonly IReleaseService _releaseService;

        public ContentController(
            IContentService contentService, 
            IPublicationService publicationService,
            IReleaseService releaseService)
        {
            _contentService = contentService;
            _publicationService = publicationService;
            _releaseService = releaseService;
        }
        
        // GET
        [HttpGet("tree")]
        public ActionResult<List<ThemeTree>> GetContentTree()
        {
            return _contentService.GetContentTree();
        }

        // GET api/publication/5
        [HttpGet("{id}")]
        public ActionResult<Publication> GetPublication(string id)
        {
            return _publicationService.GetPublication(id);
        }

        // GET api/publication/5/latest
        [HttpGet("{id}/latest")]
        public ActionResult<Release> GetLatestRelease(string id)
        {
            return _releaseService.GetLatestRelease(id);
        }

        // GET api/release/5
        [HttpGet("{id}")]
        public ActionResult<Release> GetRelease(string id)
        {
            return _releaseService.GetRelease(id);
        }
    }
}