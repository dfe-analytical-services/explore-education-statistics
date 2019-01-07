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
    public class ReleaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReleaseController(ApplicationDbContext context)
        {
            _context = context;    
        }
        
        // GET api/publication
        [HttpGet]
        public ActionResult<List<Release>> Get()
        {
            return _context.Releases.ToList();
        }
        
        // GET api/release/5
        [HttpGet("{id}")]
        public ActionResult<Release> Get(string id)
        {            
            var release =  Guid.TryParse(id, out var newGuid) ? 
                _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases).FirstOrDefault(x => x.Id == newGuid) : 
                _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases).FirstOrDefault(x => x.Slug == id);
           
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
                    PublicationId =  r.PublicationId
                }));
            }

            return release;
        }
    }
}