using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/publication
        [HttpGet]
        public ActionResult<List<Publication>> Get()
        {
            return _context.Publications.ToList();
        }

        // GET api/publication/5
        [HttpGet("{id}")]
        public ActionResult<Publication> Get(string id)
        {
            return Guid.TryParse(id, out var newGuid)
                ? _context.Publications.Include(x => x.Releases.OrderByDescending(r => r.Published)).Include(x => x.LegacyReleases)
                    .FirstOrDefault(t => t.Id == newGuid)
                : _context.Publications.Include(x => x.Releases.OrderByDescending(r => r.Published)).Include(x => x.LegacyReleases)
                    .FirstOrDefault(t => t.Slug == id);
        }

        // GET api/publication/5/latest
        [HttpGet("{id}/latest")]
        public ActionResult<Release> GetLatest(string id)
        {
            Release release;

            if (Guid.TryParse(id, out var newGuid))
            {
                release = _context.Releases.Include(x => x.Updates).Include(x => x.Publication)
                    .ThenInclude(x => x.LegacyReleases).Include(x => x.Updates).OrderBy(x => x.Published)
                    .Last(t => t.PublicationId == newGuid);
            }
            else
            {
                release = _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                    .Include(x => x.Updates).OrderBy(x => x.Published).Last(t => t.Publication.Slug == id);
            }


            if (release != null)
            {
                var releases = _context.Releases.Where(x => x.Publication.Id == release.Publication.Id).ToList();
                release.Publication.Releases = new List<Release>();
                releases.ForEach(r => release.Publication.Releases.Add(new Release()
                {
                    Id = r.Id,
                    Title = r.Title,
                    ReleaseName = r.ReleaseName,
                    Published = r.Published,
                    Slug = r.Slug,
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));
            }

            return release;
        }
    }
}