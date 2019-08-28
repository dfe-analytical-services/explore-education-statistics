using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
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