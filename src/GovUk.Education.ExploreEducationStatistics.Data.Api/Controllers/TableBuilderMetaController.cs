using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/meta")]
    [ApiController]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ISubjectMetaService _subjectMetaService;

        public TableBuilderMetaController(ISubjectMetaService subjectMetaService)
        {
            _subjectMetaService = subjectMetaService;
        }

        [HttpGet("subject/{subjectId}")]
        [BlobCache(typeof(SubjectMetaCacheKey))]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMetaAsync(Guid subjectId)
        {
            return _subjectMetaService.GetSubjectMeta(subjectId).HandleFailuresOrOk();
        }

        [HttpPost("subject")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMetaAsync(
            [FromBody] SubjectMetaQueryContext query)
        {
            return _subjectMetaService.GetSubjectMeta(query).HandleFailuresOrOk();
        }
    }
}