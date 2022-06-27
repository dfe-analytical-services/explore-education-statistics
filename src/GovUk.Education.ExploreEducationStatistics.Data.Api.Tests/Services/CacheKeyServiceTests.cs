#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class CacheKeyServiceTests
    {
        [Fact]
        public async Task CreateCacheKeyForReleaseSubjects()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var result = await service.CreateCacheKeyForReleaseSubjects(release.Id);

                var cacheKey = result.AssertRight();
                Assert.Equal(BlobContainers.PublicContent, cacheKey.Container);
                Assert.Equal(release.Id, cacheKey.ReleaseId);
                Assert.Equal("publications/publication-slug/releases/release-slug/subjects.json", cacheKey.Key);
            }
        }

        private static CacheKeyService BuildService(ContentDbContext contentDbContext)
        {
            return new CacheKeyService(contentDbContext);
        }
    }
}
