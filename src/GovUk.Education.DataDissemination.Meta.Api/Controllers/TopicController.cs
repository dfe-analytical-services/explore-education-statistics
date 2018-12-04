using System;
using System.Collections.Generic;
using GovUk.Education.DataDissemination.Meta.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GovUk.Education.DataDissemination.Meta.Api.Data;

namespace GovUk.Education.DataDissemination.Meta.Api.Controllers
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
    }
}