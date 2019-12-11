using System;
using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/meta")]
    [ApiController]
    [Authorize]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ITableBuilderSubjectMetaService _subjectMetaService;
        private readonly ILogger _logger;

        public TableBuilderMetaController(ITableBuilderSubjectMetaService subjectMetaService,
            ILogger<TableBuilderMetaController> logger)
        {
            _subjectMetaService = subjectMetaService;
            _logger = logger;
        }

        [HttpGet("subject/{subjectId}")]
        public ActionResult<TableBuilderSubjectMetaViewModel> GetSubjectMeta(Guid subjectId)
        {
            TableBuilderSubjectMetaViewModel tableBuilderSubjectMetaViewModel;

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            try
            {
                tableBuilderSubjectMetaViewModel = _subjectMetaService.GetSubjectMeta(subjectId);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }

            stopwatch.Stop();
            _logger.LogDebug("Subject meta query for Subject id {SubjectId} executed in {Time} ms", subjectId,
                stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderSubjectMetaViewModel;
        }

        [HttpPost("subject")]
        public ActionResult<TableBuilderSubjectMetaViewModel> GetSubjectMeta([FromBody] SubjectMetaQueryContext query)
        {
            TableBuilderSubjectMetaViewModel tableBuilderSubjectMetaViewModel;

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            try
            {
                tableBuilderSubjectMetaViewModel = _subjectMetaService.GetSubjectMeta(query);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }

            stopwatch.Stop();
            _logger.LogDebug("Subject meta query {Query} executed in {Time} ms", query,
                stopwatch.Elapsed.TotalMilliseconds);

            return tableBuilderSubjectMetaViewModel;
        }
    }
}