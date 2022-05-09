#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
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

            var (service, fastTrackService) = BuildServiceAndMocks(contentDbContext);

            fastTrackService
                .Setup(s => s.GetReleaseFastTrack(fastTrackId))
                .ReturnsAsync(new ReleaseFastTrack(owningRelease.Id, fastTrackId, ""));

            var result = await service.CreateCacheKeyForFastTrackResults(fastTrackId);
            VerifyAllMocks(fastTrackService);

            var cacheKey = result.AssertRight();
            Assert.Equal(BlobContainers.PublicContent, cacheKey.Container);
            var expectedCacheKeyPath =
                $"publications/{owningPublication.Slug}/releases/{owningRelease.Slug}/fast-track-results/{fastTrackId}.json";
            Assert.Equal(expectedCacheKeyPath, cacheKey.Key);
        }

        private static (
            CacheKeyService service,
            Mock<IFastTrackService> fastTrackService)
            BuildServiceAndMocks(ContentDbContext contentDbContext)
        {
            var fastTrackService = new Mock<IFastTrackService>(Strict);
            var controller = new CacheKeyService(contentDbContext, fastTrackService.Object);
            return (controller, (fastTrackService));
        }
    }
}
