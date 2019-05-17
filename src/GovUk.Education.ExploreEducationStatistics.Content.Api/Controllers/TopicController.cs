using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public TopicController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET api/topic
        [HttpGet]
        public ActionResult<List<Topic>> Get()
        {
            return _context.Topics.ToList();
        }

        // GET api/topic/5
        [HttpGet("{id}")]
        public ActionResult<Topic> Get(string id)
        {
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Topics.FirstOrDefault(t => t.Id == newGuid) : 
                _context.Topics.FirstOrDefault(t => t.Slug == id);
        }
        
        
        // GET api/topic/5/publications
        [HttpGet("{id}/publications")]
        public ActionResult<List<Publication>> GetPublications(string id)
        {
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Publications.Where(t => t.TopicId == newGuid && t.Releases.Any()).ToList() : 
                _context.Publications.Where(t => t.Topic.Slug == id && t.Releases.Any()).ToList();
        }
    }
}
