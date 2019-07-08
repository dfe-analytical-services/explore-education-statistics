using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
            // TODO DFE-866 Remove ObservationQueryContext.StartYear and EndYear when frontend updated to use TimePeriod
            query.TimePeriod = new TimePeriodQuery();
            query.TimePeriod.StartYear = query.StartYear;
            query.TimePeriod.EndYear = query.EndYear;
            if (query.SubjectId == 17)
            {
                query.TimePeriod.StartCode = TimeIdentifier.CalendarYear;
                query.TimePeriod.EndCode = TimeIdentifier.CalendarYear;              
            }
            if (query.SubjectId <= 7)
            {
                query.TimePeriod.StartCode = TimeIdentifier.SixHalfTerms;
                query.TimePeriod.EndCode = TimeIdentifier.SixHalfTerms;
            }
            else
            {
                query.TimePeriod.StartCode = TimeIdentifier.AcademicYear;
                query.TimePeriod.EndCode = TimeIdentifier.AcademicYear;
            }
            
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