#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

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

        [HttpGet("release/{releaseVersionId:guid}/meta/subject/{subjectId:guid}")]
        public Task<ActionResult<SubjectMetaViewModel>> GetSubjectMeta(Guid releaseVersionId, Guid subjectId)
        {
            return _releaseSubjectService.Find(releaseVersionId: releaseVersionId,
                    subjectId: subjectId)
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
            return await _contentPersistenceHelper.CheckEntityExists<ReleaseVersion>(releaseSubject.ReleaseVersionId,
                    q => q.Include(rv => rv.Release)
                        .ThenInclude(r => r.Publication))
                .OnSuccess(releaseVersion => new CacheableReleaseSubject(releaseSubject, releaseVersion));
        }

        [BlobCache(typeof(SubjectMetaCacheKey))]
        private Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(CacheableReleaseSubject cacheable)
        {
            return _subjectMetaService.GetSubjectMeta(cacheable.ReleaseSubject);
        }

        [HttpPost("meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            [FromBody] LocationsOrTimePeriodsQueryRequest request,
            CancellationToken cancellationToken)
        {
            return _subjectMetaService.FilterSubjectMeta(releaseVersionId: null, request, cancellationToken)
                .HandleFailuresOrOk();
        }

        [HttpPost("release/{releaseVersionId:guid}/meta/subject")]
        public Task<ActionResult<SubjectMetaViewModel>> FilterSubjectMeta(
            Guid releaseVersionId,
            [FromBody] LocationsOrTimePeriodsQueryRequest request,
            CancellationToken cancellationToken)
        {
            return _subjectMetaService.FilterSubjectMeta(releaseVersionId, request, cancellationToken)
                .HandleFailuresOrOk();
        }
    }
}
