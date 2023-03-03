#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class TableBuilderMetaController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IReleaseSubjectService _releaseSubjectService;
        private readonly ISubjectMetaService _subjectMetaService;

        public TableBuilderMetaController(
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IReleaseSubjectService releaseSubjectService,
            ISubjectMetaService subjectMetaService)
        {
            _contentPersistenceHelper = contentPersistenceHelper;
            _releaseSubjectService = releaseSubjectService;
            _subjectMetaService = subjectMetaService;
        }

        [HttpGet("meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _releaseSubjectService.Find(subjectId)
                .OnSuccess(GetCacheableReleaseSubject)
                .OnSuccess(GetSubjectMeta)
                .HandleFailuresOrOk();
        }

        [HttpGet("release/{releaseId:guid}/meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, Guid subjectId)
        {
            return _releaseSubjectService.Find(releaseId: releaseId, subjectId: subjectId)
                .OnSuccess(GetCacheableReleaseSubject)
                .OnSuccess(GetSubjectMeta)
                .HandleFailuresOrOk();
        }

        private async Task<Either<ActionResult, CacheableReleaseSubject>> GetCacheableReleaseSubject(
            ReleaseSubject releaseSubject)
        {
            // TODO EES-3363 CacheableReleaseSubject exists to provide the Publication slug not available from the ReleaseSubject
            // In future we should change the storage path for cached items to use Publication and Release id's in the
            // directory structure rather than slugs so that we don't need to lookup the Publication from the Content Release.
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseSubject.ReleaseId,
                    q => q.Include(release => release.Publication))
                .OnSuccess(release => new CacheableReleaseSubject(releaseSubject, release));
        }

        [BlobCache(typeof(SubjectMetaCacheKey))]
        private Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(CacheableReleaseSubject cacheable)
        {
            return _subjectMetaService.GetSubjectMeta(cacheable.ReleaseSubject);
        }

        [HttpPost("meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return _subjectMetaService.FilterSubjectMeta(null, query, cancellationToken)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseId:guid}/meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            Guid releaseId,
            [FromBody] ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return _subjectMetaService.FilterSubjectMeta(releaseId, query, cancellationToken)
                .HandleFailuresOrOk();
        }
    }
}
