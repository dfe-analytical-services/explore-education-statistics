using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class PublicationRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task IsPublicationPublished_FalseWhenPublicationHasNoPublishedRelease()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
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
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildPublicationRepository(contentDbContext);

            Assert.True(await service.IsPublished(publication.Id));
        }
    }

    private static PublicationRepository BuildPublicationRepository(ContentDbContext contentDbContext)
    {
        return new(contentDbContext: contentDbContext);
    }
}
