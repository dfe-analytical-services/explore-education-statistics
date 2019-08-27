using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MetaController : ControllerBase
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        // GET api/meta/timeidentifiers
        [HttpGet("timeidentifiers")]
        public ActionResult<List<TimeIdentifierCategoryModel>> GetTimeIdentifiersByCategory()
        {
            return _metaService.GetTimeIdentifiersByCategory();
        }
        
        // GET api/meta/releasetypes
        [HttpGet("releasetypes")]
        public ActionResult<List<ReleaseType>> GetReleaseTypes()
        {
            return _metaService.GetReleaseTypes();
        }
    }
}