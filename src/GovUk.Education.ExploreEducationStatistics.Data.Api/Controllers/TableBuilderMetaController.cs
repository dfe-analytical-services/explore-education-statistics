using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
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
        public ActionResult<TableBuilderSubjectMetaViewModel> GetSubjectMeta(long subjectId)
        {
            try
            {
                return _subjectMetaService.GetSubjectMeta(new SubjectMetaQueryContext
                {
                    SubjectId = subjectId
                });
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("subject")]
        public ActionResult<TableBuilderSubjectMetaViewModel> GetSubjectMeta([FromBody] SubjectMetaQueryContext query)
        {
            try
            {
                return _subjectMetaService.GetSubjectMeta(query);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}