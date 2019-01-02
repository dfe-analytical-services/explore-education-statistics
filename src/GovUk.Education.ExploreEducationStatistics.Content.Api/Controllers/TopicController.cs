using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
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
        public ActionResult<Topic> Get(Guid id)
        {
            return _context.Topics.FirstOrDefault(t => t.Id == id);
        }
        
        
        // GET api/topic/5/publications
        [HttpGet("{id}/publications")]
        public ActionResult<List<Publication>> GetPublications(Guid id)
        {
            return _context.Publications.Where(t => t.TopicId == id).ToList();
        }
    }
}
