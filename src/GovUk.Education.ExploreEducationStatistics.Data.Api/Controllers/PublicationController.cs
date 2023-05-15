#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IReleaseService _releaseService;
        private readonly ICacheKeyService _cacheKeyService;

        public PublicationController(
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IReleaseService releaseService,
            ICacheKeyService cacheKeyService)
        {
            _contentPersistenceHelper = contentPersistenceHelper;
            _releaseService = releaseService;
            _cacheKeyService = cacheKeyService;
        }

        [HttpGet("publications/{publicationId}/subjects")]
        public async Task<ActionResult<List<SubjectViewModel>>> ListLatestReleaseSubjects(Guid publicationId)
        {
            return await GetLatestPublishedReleaseId(publicationId)
                .OnSuccess(_cacheKeyService.CreateCacheKeyForReleaseSubjects)
                .OnSuccess(ListLatestReleaseSubjects)
                .HandleFailuresOrOk();
        }

        [HttpGet("publications/{publicationId}/featured-tables")]
        public async Task<ActionResult<List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(
            Guid publicationId)
        {
            return await GetLatestPublishedReleaseId(publicationId)
                .OnSuccess(_releaseService.ListFeaturedTables)
                .HandleFailuresOrOk();
        }

        private Task<Either<ActionResult, Guid>> GetLatestPublishedReleaseId(Guid publicationId)
        {
            return _contentPersistenceHelper.CheckEntityExists<Publication>(publicationId)
                .OnSuccess(publication => publication.LatestPublishedReleaseId ??
                                          new Either<ActionResult, Guid>(new NotFoundResult()));
        }

        [BlobCache(typeof(ReleaseSubjectsCacheKey))]
        private async Task<Either<ActionResult, List<SubjectViewModel>>> ListLatestReleaseSubjects(
            ReleaseSubjectsCacheKey cacheKey)
        {
            return await _releaseService.ListSubjects(cacheKey.ReleaseId);
        }
    }
}
