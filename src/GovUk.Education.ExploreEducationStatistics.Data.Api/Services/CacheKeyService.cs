using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class CacheKeyService : ICacheKeyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IFastTrackService _fastTrackService;

        public CacheKeyService(
            ContentDbContext contentDbContext,
            IFastTrackService fastTrackService)
        {
            _contentDbContext = contentDbContext;
            _fastTrackService = fastTrackService;
        }

        public Task<Either<ActionResult, FastTrackResultsCacheKey>> CreateCacheKeyForFastTrackResults(Guid fastTrackId)
        {
            return _fastTrackService
                .GetReleaseFastTrack(fastTrackId)
                .OnSuccess(async fastTrack =>
                {
                    var owningRelease = await _contentDbContext
                        .Releases
                        .Include(release => release.Publication)
                        .SingleAsync(release => release.Id == fastTrack.ReleaseId);

                    return new FastTrackResultsCacheKey(owningRelease.Publication.Slug, owningRelease.Slug,
                        fastTrackId);
                });
        }

        public async Task<Either<ActionResult, ReleaseSubjectsCacheKey>> CreateCacheKeyForReleaseSubjects(
            Guid releaseId)
        {
            var release = await _contentDbContext
                .Releases
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseId);

            return new ReleaseSubjectsCacheKey(release.Publication.Slug, release.Slug, release.Id);
        }
    }
}
