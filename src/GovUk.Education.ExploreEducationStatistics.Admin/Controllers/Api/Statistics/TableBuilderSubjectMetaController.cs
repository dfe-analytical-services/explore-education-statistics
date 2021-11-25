#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ISubjectMetaService _subjectMetaService;

        public TableBuilderMetaController(ISubjectMetaService subjectMetaService)
        {
            _subjectMetaService = subjectMetaService;
        }

        [HttpGet("data/meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _subjectMetaService.GetSubjectMetaRestricted(subjectId)
                .HandleFailuresOrOk();
        }

        [HttpPost("data/meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(
            [FromBody] SubjectMetaQueryContext query)
        {
            return _subjectMetaService.GetSubjectMetaRestricted(query)
                .HandleFailuresOrOk();
        }
    }
}
