using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPublicationTests : PublicationsServiceTests
    {
        [Fact]
        public async Task GetPublication_WhenPublicationExists_ReturnsExpectedPublication()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.Multiple(() =>
                {
                    Assert.Equal(publication.Id, result.Id);
                    Assert.Equal(publication.Slug, result.Slug);
                    Assert.Equal(publication.Summary, result.Summary);
                    Assert.Equal(publication.Title, result.Title);
                    Assert.Equal(publication.Contact.Id, result.Contact.Id);
                    Assert.Equal(publication.Contact.ContactName, result.Contact.ContactName);
                    Assert.Equal(publication.Contact.ContactTelNo, result.Contact.ContactTelNo);
                    Assert.Equal(publication.Contact.TeamEmail, result.Contact.TeamEmail);
                    Assert.Equal(publication.Contact.TeamName, result.Contact.TeamName);
                    Assert.Equal(publication.Releases[0].Id, result.LatestRelease.Id);
                    Assert.Equal(publication.Releases[0].Slug, result.LatestRelease.Slug);
                    Assert.Equal(publication.Releases[0].Title, result.LatestRelease.Title);
                    Assert.Null(result.SupersededByPublication);
                    Assert.Equal(publication.Theme.Id, result.Theme.Id);
                    Assert.Equal(publication.Theme.Summary, result.Theme.Summary);
                    Assert.Equal(publication.Theme.Title, result.Theme.Title);
                });
            }
        }

        [Fact]
        public async Task GetPublication_WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublication(publicationSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task GetPublication_WhenPublicationHasNoReleases_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme());

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetPublication_WhenPublicationHasNoPublishedRelease_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task
            GetPublication_WhenPublicationHasMultiplePublishedReleases_ReturnsPublicationWithExpectedLatestRelease()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease(publishedVersions: 1),
                    _dataFixture.DefaultRelease(publishedVersions: 1)
                ]);
            var latestPublishedRelease = publication.LatestPublishedReleaseVersion!.Release;

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.Multiple(() =>
                {
                    Assert.Equal(latestPublishedRelease.Id, result.LatestRelease.Id);
                    Assert.Equal(latestPublishedRelease.Slug, result.LatestRelease.Slug);
                    Assert.Equal(latestPublishedRelease.Title, result.LatestRelease.Title);
                });
            }
        }

        [Fact]
        public async Task
            GetPublication_WhenSupersedingPublicationHasPublishedRelease_ReturnsPublicationWithExpectedSupersededByPublication()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(_dataFixture
                    .DefaultPublication()
                    .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]));
            var supersedingPublication = publication.SupersededBy!;

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.NotNull(result.SupersededByPublication);
                Assert.Multiple(() =>
                {
                    Assert.Equal(supersedingPublication.Id, result.SupersededByPublication.Id);
                    Assert.Equal(supersedingPublication.Slug, result.SupersededByPublication.Slug);
                    Assert.Equal(supersedingPublication.Title, result.SupersededByPublication.Title);
                });
            }
        }

        [Fact]
        public async Task
            GetPublication_WhenSupersedingPublicationHasNoPublishedRelease_ReturnsPublicationWithNoSupersededByPublication()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(_dataFixture.DefaultPublication());

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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.Null(result.SupersededByPublication);
            }
        }
    }

    private static PublicationsService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
