#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class PublicationRepositoryTests
{
    [Fact]
    public async Task IsPublicationPublished_FalseWhenPublicationHasNoPublishedRelease()
    {
        var publication = new Publication
        {
            LatestPublishedReleaseId = null
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildPublicationRepository(contentDbContext);

            Assert.False(await service.IsPublished(publication.Id));
        }
    }

    [Fact]
    public async Task IsPublicationPublished_TrueWhenPublicationHasPublishedRelease()
    {
        var publication = new Publication
        {
            LatestPublishedReleaseId = Guid.NewGuid()
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildPublicationRepository(contentDbContext);

            Assert.True(await service.IsPublished(publication.Id));
        }
    }

    private static PublicationRepository BuildPublicationRepository(
        ContentDbContext contentDbContext)
    {
        return new(
            contentDbContext: contentDbContext
        );
    }
}
