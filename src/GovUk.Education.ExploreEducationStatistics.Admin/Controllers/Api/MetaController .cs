using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("/api")]
    public class MetaController : ControllerBase
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        // GET api/meta/timeidentifiers
        [HttpGet("/meta/timeidentifiers")]
        [AllowAnonymous] // Anyone can see the time identifiers
        public ActionResult<List<TimeIdentifierCategoryModel>> GetTimeIdentifiersByCategory()
        {
            return _metaService.GetTimeIdentifiersByCategory();
        }
    }
}