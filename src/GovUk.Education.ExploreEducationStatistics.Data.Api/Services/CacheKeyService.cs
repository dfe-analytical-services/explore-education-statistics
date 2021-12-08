using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class CacheKeyService : ICacheKeyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFastTrackService _fastTrackService;

        public CacheKeyService(
            ContentDbContext contentDbContext, 
            StatisticsDbContext statisticsDbContext,
            IFastTrackService fastTrackService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
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

                    return new FastTrackResultsCacheKey(owningRelease.Publication.Slug, owningRelease.Slug, fastTrackId);
                });
        }

        public async Task<Either<ActionResult, SubjectMetaCacheKey>> CreateCacheKeyForSubjectMeta(Guid subjectId)
        {
            var publishedReleaseSubjects = await _statisticsDbContext
                .ReleaseSubject
                .Include(releaseSubject => releaseSubject.Release)
                .Where(releaseSubject => releaseSubject.SubjectId == subjectId && releaseSubject.Release.Published.HasValue)
                .ToListAsync();

            var liveReleaseSubjects = publishedReleaseSubjects
                .Where(releaseSubject => releaseSubject.Release.Live)
                .ToList();

            var previousReleaseVersionIds = liveReleaseSubjects
                .Select(releaseSubject => releaseSubject.Release.PreviousVersionId);

            var latestReleaseSubject = liveReleaseSubjects.SingleOrDefault(
                releaseSubject => !previousReleaseVersionIds.Contains(releaseSubject.Release.Id));

            if (latestReleaseSubject == null)
            {
                return new NotFoundResult();
            }

            var publication = await _contentDbContext
                .Publications
                .SingleAsync(publication => publication.Id == latestReleaseSubject.Release.PublicationId);

            return new SubjectMetaCacheKey(publication.Slug, latestReleaseSubject.Release.Slug, subjectId);
        }
        
        public async Task<Either<ActionResult, ReleaseSubjectsCacheKey>> CreateCacheKeyForReleaseSubjects(Guid releaseId)
        {
            var release = await _contentDbContext
                .Releases
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseId);

            return new ReleaseSubjectsCacheKey(release.Publication.Slug, release.Slug, release.Id);
        }
    }
}