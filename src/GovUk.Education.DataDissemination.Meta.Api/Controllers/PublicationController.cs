using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.DataDissemination.Meta.Api.Data;
using GovUk.Education.DataDissemination.Meta.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.DataDissemination.Meta.Api.Controllers
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
        public ActionResult<Publication> Get(Guid id)
        {
            return _context.Publications.FirstOrDefault(t => t.Id == id);
        }
    }
}