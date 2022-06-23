#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
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

        [HttpGet("data/release/{releaseId:guid}/meta/subject/{subjectId:guid}")]
        [BlobCache(typeof(PrivateSubjectMetaCacheKey))]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, Guid subjectId)
        {
            return _subjectMetaService.GetSubjectMeta(releaseId: releaseId, subjectId: subjectId)
                .HandleFailuresOrOk();
        }

        [HttpPost("data/release/{releaseId:guid}/meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            Guid releaseId,
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return _subjectMetaService.FilterSubjectMeta(releaseId, query, cancellationToken)
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
    }
}
