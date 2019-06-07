using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPublicationService _publicationService;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public ContentController(
            IContentService contentService,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IPublicationService publicationService,
            IReleaseService releaseService)
        {
            _mapper = mapper;
            _contentService = contentService;
            _fileStorageService = fileStorageService;
            _publicationService = publicationService;
            _releaseService = releaseService;
        }
        
        // GET api/content/tree
        [HttpGet("tree")]
        public ActionResult<List<ThemeTree>> GetContentTree()
        {
            var tree = _contentService.GetContentTree();
            
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

        // GET api/content/publication/5/latest
        [HttpGet("publication/{publicationSlug}/latest")]
        public ActionResult<ReleaseViewModel> GetLatestRelease(string publicationSlug)
        {
            var release = _releaseService.GetLatestRelease(publicationSlug);
            var listFiles = _fileStorageService.ListFiles(publicationSlug, release.Slug).ToList();

            if (release != null)
            {
                var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                releaseViewModel.DataFiles = listFiles;
                return releaseViewModel;
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