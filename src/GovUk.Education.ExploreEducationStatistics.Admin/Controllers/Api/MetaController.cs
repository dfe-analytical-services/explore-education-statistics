#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("api/meta")]
    [ApiController]
    [Authorize]
    public class MetaController : ControllerBase
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        [HttpGet("timeidentifiers")]
        public ActionResult<List<TimeIdentifierCategoryModel>> GetTimeIdentifiersByCategory()
        {
            return _metaService.GetTimeIdentifiersByCategory();
        }
    }
}
