using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases;

public abstract class ReleaseUpdatesServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPaginatedUpdatesForReleaseTests : ReleaseUpdatesServiceTests
    {
        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = "publication-that-does-not-exist",
                ReleaseSlug = "test-release"
            };

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPaginatedUpdatesForRelease(request);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = publication.Slug,
                ReleaseSlug = "release-that-does-not-exist"
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPaginatedUpdatesForRelease(request);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenReleaseHasNoPublishedVersion_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);
            var release = publication.Releases[0];
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = publication.Slug,
                ReleaseSlug = release.Slug
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPaginatedUpdatesForRelease(request);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static ReleaseUpdatesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
