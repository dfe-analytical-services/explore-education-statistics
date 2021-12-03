using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache
{
    public class CacheKeyService : ICacheKeyService
    {
        // private readonly ContentDbContext _contentDbContext;
        // private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;

        public CacheKeyService(
            // ContentDbContext contentDbContext, 
            // StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper)
        {
            // _contentDbContext = contentDbContext;
            // _statisticsDbContext = statisticsDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
        }
        
        public async Task<Either<ActionResult, DataBlockTableResultCacheKey>> CreateCacheKeyForDataBlock(Guid releaseId, Guid dataBlockId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<ReleaseContentBlock>(query => query
                    .Include(rcb => rcb.Release)
                    .ThenInclude(release => release.Publication)
                    .Include(rcb => rcb.ContentBlock)
                    .Where(rcb => rcb.ContentBlockId == dataBlockId && rcb.ReleaseId == releaseId))
                .OnSuccess(releaseContentBlock => new DataBlockTableResultCacheKey(releaseContentBlock));
        }

        // public async Task<Either<ActionResult, SubjectMetaCacheKey>> CreateCacheKeyForSubjectMeta(Guid subjectId)
        // {
        //     var publishedReleaseSubjects = await Queryable.Where(_statisticsDbContext
        //             .ReleaseSubject
        //             .Include(releaseSubject => releaseSubject.Release), releaseSubject => releaseSubject.SubjectId == subjectId && releaseSubject.Release.Published.HasValue)
        //         .ToListAsync();
        //
        //     var liveReleaseSubjects = Enumerable.ToList(Enumerable.Where(publishedReleaseSubjects, releaseSubject => releaseSubject.Release.Live));
        //
        //     var previousReleaseVersionIds = Enumerable.Select(liveReleaseSubjects, releaseSubject => releaseSubject.Release.PreviousVersionId);
        //
        //     var latestReleaseSubject = Enumerable.SingleOrDefault(liveReleaseSubjects, releaseSubject => !Enumerable.Contains(previousReleaseVersionIds, releaseSubject.Release.Id));
        //
        //     if (latestReleaseSubject == null)
        //     {
        //         return new NotFoundResult();
        //     }
        //
        //     var publication = await _contentDbContext
        //         .Publications
        //         .SingleAsync(publication => publication.Id == latestReleaseSubject.Release.PublicationId);
        //
        //     return new SubjectMetaCacheKey(publication.Slug, latestReleaseSubject.Release.Slug, subjectId);
        // }
        //
        // public async Task<Either<ActionResult, ReleaseSubjectsCacheKey>> CreateCacheKeyForReleaseSubjects(Guid releaseId)
        // {
        //     var release = await _contentDbContext
        //         .Releases
        //         .Include(release => release.Publication)
        //         .SingleAsync(release => release.Id == releaseId);
        //
        //     return new ReleaseSubjectsCacheKey(release.Publication.Slug, release.Slug, release.Id);
        // }
    }
}