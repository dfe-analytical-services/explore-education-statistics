using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.DataDissemination.Meta.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.DataDissemination.Meta.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly List<Publication> _publications = new List<Publication>
        {
            new Publication() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England"},
            new Publication() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England: autumn term"},
            new Publication() {Id = Guid.NewGuid(), Title = "Pupil absence in schools in England: autumn and spring"}
        };

        // GET api/publication
        [HttpGet]
        public ActionResult<List<Publication>> Get()
        {
            return _publications;
        }

        // GET api/publication/5
        [HttpGet("{id}")]
        public ActionResult<Publication> Get(Guid id)
        {
            return _publications.FirstOrDefault();
        }
    }
}