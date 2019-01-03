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
    public class ThemeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ThemeController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET api/theme
        [HttpGet]
        public ActionResult<List<Theme>> Get()
        {
            return _context.Themes.ToList();
        }

        // GET api/theme/5
        [HttpGet("{id}")]
        public ActionResult<Theme> Get(string id)
        {
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Themes.FirstOrDefault(t => t.Id == newGuid) : 
                _context.Themes.FirstOrDefault(t => t.Slug == id);
        }
        
        // GET api/theme/5/topics
        [HttpGet("{id}/topics")]
        public ActionResult<List<Topic>> GetTopics(string id)
        {
            
            return Guid.TryParse(id, out var newGuid) ? 
                _context.Topics.Where(t => t.ThemeId == newGuid).ToList() : 
                _context.Topics.Where(t => t.Slug == id).ToList();
        }
    }
}
