using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.DataDissemination.Meta.Api.Data;
using GovUk.Education.DataDissemination.Meta.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.DataDissemination.Meta.Api.Controllers
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
        public ActionResult<Theme> Get(Guid id)
        {
            return _context.Themes.FirstOrDefault(t => t.Id == id);
        }
    }
}