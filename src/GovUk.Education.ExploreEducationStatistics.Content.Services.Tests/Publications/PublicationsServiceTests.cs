using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPublicationTests : PublicationsServiceTests
    {
        [Fact]
        public async Task WhenPublicationExists_ReturnsExpectedPublication()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
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
                    AssertPublicationNextReleaseDateEqual(
                        publication.Releases[0].Versions[0].NextReleaseDate,
                        result.NextReleaseDate
                    );
                    Assert.Null(result.SupersededByPublication);
                    Assert.Equal(publication.Theme.Id, result.Theme.Id);
                    Assert.Equal(publication.Theme.Summary, result.Theme.Summary);
                    Assert.Equal(publication.Theme.Title, result.Theme.Title);
                });
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
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
        public async Task WhenPublicationHasNoReleases_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme());

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
        public async Task WhenPublicationHasNoPublishedRelease_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
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
        public async Task WhenPublicationHasMultiplePublishedReleases_ReturnsPublicationWithExpectedLatestRelease()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1),
                        _dataFixture.DefaultRelease(publishedVersions: 1),
                    ]
                );
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
        public async Task WhenSupersedingPublicationHasPublishedRelease_ReturnsPublicationWithExpectedSupersededByPublication()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(
                    _dataFixture.DefaultPublication().WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                );
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
        public async Task WhenSupersedingPublicationHasNoPublishedRelease_ReturnsPublicationWithNoSupersededByPublication()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
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

        [Fact]
        public async Task WhenLatestReleaseVersionHasNoNextReleaseDate_ReturnsPublicationWithNoNextReleaseDate()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            publication.Releases[0].Versions[0].NextReleaseDate = null;

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
                Assert.Null(result.NextReleaseDate);
            }
        }

        [Theory]
        [InlineData("2025", null, null)]
        [InlineData("2025", "9", null)]
        [InlineData("2025", "9", "5")]
        public async Task WhenLatestReleaseVersionHasPartialNextReleaseDate_ReturnsPublicationWithExpectedNextReleaseDate(
            string year,
            string? month,
            string? day
        )
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            publication.Releases[0].Versions[0].NextReleaseDate = new PartialDate
            {
                Year = year,
                Month = month ?? string.Empty,
                Day = day ?? string.Empty,
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
                var outcome = await sut.GetPublication(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                AssertPublicationNextReleaseDateEqual(
                    publication.Releases[0].Versions[0].NextReleaseDate,
                    result.NextReleaseDate
                );
            }
        }

        private static void AssertPublicationNextReleaseDateEqual(
            PartialDate? expected,
            PublicationNextReleaseDateDto? actual
        )
        {
            if (expected == null || actual == null)
            {
                Assert.Null(expected);
                Assert.Null(actual);
                return;
            }

            Assert.Multiple(() =>
            {
                if (int.TryParse(expected.Year, out var expectedYear))
                {
                    Assert.Equal(expectedYear, actual.Year);
                }

                if (int.TryParse(expected.Month, out var expectedMonth))
                {
                    Assert.Equal(expectedMonth, actual.Month);
                }

                if (int.TryParse(expected.Day, out var expectedDay))
                {
                    Assert.Equal(expectedDay, actual.Day);
                }
            });
        }
    }

    private static PublicationsService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
