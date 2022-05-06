#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ISubjectMetaService _subjectMetaService;

        public TableBuilderMetaController(IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ISubjectMetaService subjectMetaService)
        {
            _persistenceHelper = persistenceHelper;
            _subjectMetaService = subjectMetaService;
        }

        [HttpGet("data/release/{releaseId:guid}/meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, Guid subjectId)
        {
            return CheckReleaseSubjectExists(releaseId, subjectId)
                .OnSuccess(_subjectMetaService.GetSubjectMeta)
                .HandleFailuresOrOk();
        }

        [HttpPost("data/release/{releaseId:guid}/meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            Guid releaseId,
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return CheckReleaseSubjectExists(releaseId, query.SubjectId)
                .OnSuccess(releaseSubject =>
                    _subjectMetaService.FilterSubjectMeta(releaseSubject, query, cancellationToken))
                .HandleFailuresOrOk();
        }

        [HttpPatch("data/release/{releaseId:guid}/meta/subject/{subjectId:guid}/filters")]
        public Task<ActionResult<Unit>> UpdateFilters(
            Guid releaseId,
            Guid subjectId,
            List<FilterUpdateViewModel> request)
        {
            return _subjectMetaService.UpdateSubjectFilters(releaseId, subjectId, request)
                .HandleFailuresOrOk();
        }

        [HttpPatch("data/release/{releaseId:guid}/meta/subject/{subjectId:guid}/indicators")]
        public Task<ActionResult<Unit>> UpdateIndicators(
            Guid releaseId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request)
        {
            return _subjectMetaService.UpdateSubjectIndicators(releaseId, subjectId, request)
                .HandleFailuresOrOk();
        }

        private async Task<Either<ActionResult, ReleaseSubject>> CheckReleaseSubjectExists(Guid releaseId,
            Guid subjectId)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseSubject>(
                query => query
                    .Include(rs => rs.Subject)
                    .Where(rs => rs.ReleaseId == releaseId
                                 && rs.SubjectId == subjectId));
        }
    }
}
