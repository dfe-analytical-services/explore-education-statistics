using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
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
            const string publicationSlug = "publication-that-does-not-exist";
            const string releaseSlug = "test-release";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPaginatedUpdatesForRelease(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();
            const string releaseSlug = "release-that-does-not-exist";

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
                var outcome = await sut.GetPaginatedUpdatesForRelease(
                    publicationSlug: publication.Slug,
                    releaseSlug: releaseSlug);

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
                var outcome = await sut.GetPaginatedUpdatesForRelease(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static ReleaseUpdatesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
