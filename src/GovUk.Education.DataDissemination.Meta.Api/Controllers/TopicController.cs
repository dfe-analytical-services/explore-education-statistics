using System;
using System.Collections.Generic;
using GovUk.Education.DataDissemination.Meta.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GovUk.Education.DataDissemination.Meta.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly List<Topic> _topics = new List<Topic>
        {
            new Topic() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England"},

            new Topic() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England: autumn term"},
            new Topic() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England: autumn and spring"}
        };
        
        // GET api/topic
        [HttpGet]
        public ActionResult<List<Topic>> Get()
        {
            return _topics;
        }

        // GET api/topic/5
        [HttpGet("{id}")]
        public ActionResult<Topic> Get(Guid id)
        {
            return _topics.FirstOrDefault();
        }
    }
}