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
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Publications.Include(x => x.LegacyReleases).FirstOrDefault(t => t.Id == newGuid) : 
                _context.Publications.Include(x => x.LegacyReleases).FirstOrDefault(t => t.Slug == id);
        }
        
        // GET api/publication/5
        [HttpGet("{id}/latest")]
        public ActionResult<Release> GetLatest(string id)
        {
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases).FirstOrDefault(t => t.PublicationId == newGuid) : 
                _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases).FirstOrDefault(t => t.Publication.Slug == id);
        }
    }
}
