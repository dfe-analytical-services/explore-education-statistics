#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly ISubjectMetaService _subjectMetaService;
        private readonly ICacheKeyService _cacheKeyService;

        public TableBuilderMetaController(
            ISubjectMetaService subjectMetaService,
            ICacheKeyService cacheKeyService)
        {
            _subjectMetaService = subjectMetaService;
            _cacheKeyService = cacheKeyService;
        }

        [HttpGet("meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _cacheKeyService
                .CreateCacheKeyForSubjectMeta(subjectId)
                .OnSuccess(GetSubjectMeta)
                .HandleFailuresOrOk();
        }

        [BlobCache(typeof(SubjectMetaCacheKey))]
        private Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(SubjectMetaCacheKey cacheKey)
        {
            return _subjectMetaService.GetSubjectMeta(cacheKey.SubjectId);
        }

        [HttpPost("meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(
            [FromBody] ObservationQueryContext query)
        {
            return _subjectMetaService.GetSubjectMeta(query).HandleFailuresOrOk();
        }
    }
}
