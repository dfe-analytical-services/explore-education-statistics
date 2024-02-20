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

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class CacheKeyServiceTests
    {
        [Fact]
        public async Task CreateCacheKeyForReleaseSubjects()
        {
            var releaseVersion = new ReleaseVersion
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
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var result = await service.CreateCacheKeyForReleaseSubjects(releaseVersion.Id);

                var cacheKey = result.AssertRight();
                Assert.Equal(BlobContainers.PublicContent, cacheKey.Container);
                Assert.Equal(releaseVersion.Id, cacheKey.ReleaseVersionId);
                Assert.Equal("publications/publication-slug/releases/release-slug/subjects.json", cacheKey.Key);
            }
        }

        private static CacheKeyService BuildService(ContentDbContext contentDbContext)
        {
            return new CacheKeyService(contentDbContext);
        }
    }
}
