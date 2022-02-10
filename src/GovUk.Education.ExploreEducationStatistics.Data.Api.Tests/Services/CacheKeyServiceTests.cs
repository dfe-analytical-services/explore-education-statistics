using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class CacheKeyServiceTests
    {
        [Fact]
        public async Task CreateCacheKeyForFastTrackResults()
        {
            var fastTrackId = Guid.NewGuid();
            var owningPublication = new Publication
            {
                Id = Guid.NewGuid(),
                Slug = "the-publication-slug"
            };
            var owningRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = owningPublication,
                Slug = "the-release-slug"
            };
            
            await using var contentDbContext = InMemoryContentDbContext();
            await contentDbContext.Releases.AddAsync(owningRelease);
            await contentDbContext.SaveChangesAsync();
            
            var (service, fastTrackService) = BuildServiceAndMocks(contentDbContext, null);

            fastTrackService
                .Setup(s => s.GetReleaseFastTrack(fastTrackId))
                .ReturnsAsync(new ReleaseFastTrack(owningRelease.Id, fastTrackId, ""));
                
            var result = await service.CreateCacheKeyForFastTrackResults(fastTrackId);
            VerifyAllMocks(fastTrackService);

            var cacheKey = result.AssertRight();
            Assert.Equal(BlobContainers.PublicContent, cacheKey.Container);
            var expectedCacheKeyPath = $"publications/{owningPublication.Slug}/releases/{owningRelease.Slug}/fast-track-results/{fastTrackId}.json";
            Assert.Equal(expectedCacheKeyPath, cacheKey.Key);
        }
        
        [Fact]
        public async Task CreateCacheKeyForSubjectMetas()
        {
            var subjectId = Guid.NewGuid();
            var owningPublication = new Publication
            {
                Id = Guid.NewGuid(),
                Slug = "the-publication-slug"
            };
            
            // Create a number of ReleaseSubject entries - entries that tie a particular Subject to the Releases that
            // use it.  One of these is the current Live Release, and it's this one that we expect to be used for
            // generating the cache key slug.
            var owningPublishedPreviousRelease = new Model.Release
            {
                Id = Guid.NewGuid(),
                Slug = "the-release-slug-1",
                Published = DateTime.UtcNow.AddDays(-2),
                PublicationId = owningPublication.Id
                
            };
            var owningPublishedLiveRelease = new Model.Release
            {
                Id = Guid.NewGuid(),
                Slug = "the-release-slug-2",
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = owningPublishedPreviousRelease.Id,
                PublicationId = owningPublication.Id
            };
            // This represents an Amendment of the current Live Release effectively, but it also has a future
            // "Published" date set.  It's not considered Live as we check the Published date against the current
            // date time.
            var owningPublishedFutureRelease = new Model.Release
            {
                Id = Guid.NewGuid(),
                Slug = "the-release-slug-3",
                Published = DateTime.UtcNow.AddDays(1),
                PreviousVersionId = owningPublishedLiveRelease.Id,
                PublicationId = owningPublication.Id
            };
            var unrelatedLiveRelease = new Model.Release
            {
                Id = Guid.NewGuid(),
                Slug = "the-release-slug-4",
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };
            
            await using var contentDbContext = InMemoryContentDbContext();
            await contentDbContext.Publications.AddAsync(owningPublication);
            await contentDbContext.SaveChangesAsync();
            
            await using var statisticsDbContext = InMemoryPublicStatisticsDbContext();
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(
                new ReleaseSubject
                {
                    SubjectId = subjectId,
                    Release = owningPublishedPreviousRelease
                },
                new ReleaseSubject
                {
                    SubjectId = subjectId,
                    Release = owningPublishedLiveRelease
                },
                new ReleaseSubject
                {
                    SubjectId = subjectId,
                    Release = owningPublishedFutureRelease
                },
                new ReleaseSubject
                {
                    SubjectId = Guid.NewGuid(),
                    Release = unrelatedLiveRelease
                });
            await statisticsDbContext.SaveChangesAsync();
            
            var (service, _) = BuildServiceAndMocks(contentDbContext, statisticsDbContext);

            var result = await service.CreateCacheKeyForSubjectMeta(subjectId);

            var cacheKey = result.AssertRight();
            Assert.Equal(BlobContainers.PublicContent, cacheKey.Container);
            var expectedCacheKeyPath = $"publications/{owningPublication.Slug}/releases/{owningPublishedLiveRelease.Slug}/subject-meta/{subjectId}.json";
            Assert.Equal(expectedCacheKeyPath, cacheKey.Key);
        }
        
        private (
            CacheKeyService service,
            Mock<IFastTrackService> fastTrackService) 
            BuildServiceAndMocks(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext)
        {
            var fastTrackService = new Mock<IFastTrackService>(Strict);
            var controller = new CacheKeyService(contentDbContext, statisticsDbContext, fastTrackService.Object);
            return (controller, (fastTrackService));
        }
    }
}
