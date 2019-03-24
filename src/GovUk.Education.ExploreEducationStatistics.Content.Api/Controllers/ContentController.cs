using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : Controller
    {
        private readonly IContentService _service;
        public ContentController(IContentService service)
        {
            _service = service;    
        }
        
        // GET
        [HttpGet("tree")]
        public ActionResult<List<ThemeTree>> GetContentTree()
        {
            return _service.GetContentTree();
        }
    }
}