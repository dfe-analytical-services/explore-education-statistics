using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/meta")]
    [ApiController]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ITableBuilderSubjectMetaService _subjectMetaService;

        public TableBuilderMetaController(ITableBuilderSubjectMetaService subjectMetaService)
        {
            _subjectMetaService = subjectMetaService;
        }

        [HttpGet("subject/{subjectId}")]
        public Task<ActionResult<TableBuilderSubjectMetaViewModel>> GetSubjectMetaAsync(Guid subjectId)
        {
            return _subjectMetaService.GetSubjectMeta(subjectId).HandleFailuresOrOk();
        }

        [HttpPost("subject")]
        public Task<ActionResult<TableBuilderSubjectMetaViewModel>> GetSubjectMetaAsync(
            [FromBody] SubjectMetaQueryContext query)
        {
            return _subjectMetaService.GetSubjectMeta(query).HandleFailuresOrOk();
        }
    }
}