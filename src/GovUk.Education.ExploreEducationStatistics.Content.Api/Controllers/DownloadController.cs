using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers
{
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _service;
        public DownloadController(IDownloadService service)
        {
            _service = service;
        }

        // GET
        [HttpGet("tree")]
        public ActionResult<List<ThemeTree>> GetDownloadTree()
        {
            var tree = _service.GetTree();

            if (tree.Any())
            {
                return tree;
            }

            return NoContent();
        }
    }
}