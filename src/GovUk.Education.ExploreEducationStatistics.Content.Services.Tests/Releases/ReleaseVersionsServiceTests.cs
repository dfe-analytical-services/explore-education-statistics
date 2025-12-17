using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases;

public abstract class ReleaseVersionsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetReleaseVersionSummaryTests : ReleaseVersionsServiceTests
    {
        [Fact]
        public async Task WhenPublicationAndReleaseExist_ReturnsExpectedSummary()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1).WithLabel("Final")]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.Updates = _dataFixture
                .DefaultUpdate()
                .WithReleaseVersionId(releaseVersion.Id)
                .GenerateList(2);

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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug
                );

                // Assert
                var result = outcome.AssertRight();

                // Update count should include the 'First published' entry
                var expectedUpdateCount = releaseVersion.Updates.Count + 1;

                Assert.Multiple(() =>
                {
                    Assert.Equal(releaseVersion.Id, result.Id);
                    Assert.Equal(releaseVersion.ReleaseId, result.ReleaseId);
                    Assert.True(result.IsLatestRelease);
                    Assert.Equal(release.Label, result.Label);
                    Assert.Equal(releaseVersion.Published, result.LastUpdated);
                    // TODO EES-6414 'Published' should be the published display date
                    Assert.Equal(releaseVersion.Published, result.Published);
                    Assert.Empty(result.PublishingOrganisations);
                    Assert.Equal(release.Slug, result.Slug);
                    Assert.Equal(release.Title, result.Title);
                    Assert.Equal(release.TimePeriodCoverage.GetEnumLabel(), result.CoverageTitle);
                    Assert.Equal(release.YearTitle, result.YearTitle);
                    Assert.Equal(releaseVersion.Type, result.Type);
                    Assert.Equal(releaseVersion.PreReleaseAccessList, result.PreReleaseAccessList);
                    Assert.Equal(expectedUpdateCount, result.UpdateCount);
                });
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            const string releaseSlug = "test-release";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseVersionSummary(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug
            );

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseDoesNotExist_ReturnsNotFound()
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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: releaseSlug
                );

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenReleaseHasNoPublishedVersion_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug
                );

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersions_ReturnsLatestPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)]);
            var release = publication.Releases[0];

            // Ensure the generated release versions have different published dates
            Assert.True(
                release.Versions[0].Published < release.Versions[1].Published,
                "The first version should have an earlier published date than the second version"
            );

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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug
                );

                // Assert
                var result = outcome.AssertRight();
                Assert.Multiple(() =>
                {
                    Assert.Equal(release.Versions[1].Id, result.Id);
                    Assert.Equal(release.Versions[1].Published, result.LastUpdated);
                });
            }
        }

        [Fact]
        public async Task WhenReleaseIsNotLatestForPublication_ReturnsIsLatestReleaseFalse()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                    ]
                );

            var nonLatestRelease = publication.Releases.Single(r => r.Year == 2024);
            var latestRelease = publication.Releases.Single(r => r.Year == 2025);

            // Ensure the generated data has the expected latest published release version
            Assert.Equal(latestRelease.Versions[0].Id, publication.LatestPublishedReleaseVersionId);

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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: nonLatestRelease.Slug
                );

                // Assert
                var result = outcome.AssertRight();
                Assert.Multiple(() =>
                {
                    Assert.Equal(nonLatestRelease.Versions[0].Id, result.Id);
                    Assert.False(result.IsLatestRelease);
                });
            }
        }

        [Fact]
        public async Task WhenPublishingOrganisationsExist_ReturnsPublishingOrganisationsOrderedByTitle()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.PublishingOrganisations = _dataFixture
                .DefaultOrganisation()
                .ForIndex(0, s => s.SetTitle("Organisation C"))
                .ForIndex(1, s => s.SetTitle("Organisation A"))
                .ForIndex(2, s => s.SetTitle("Organisation B"))
                .GenerateList(3);

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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug
                );

                // Assert
                var result = outcome.AssertRight();

                var expectedOrganisations = releaseVersion.PublishingOrganisations.OrderBy(o => o.Title).ToArray();

                Assert.Equal(expectedOrganisations.Length, result.PublishingOrganisations.Length);
                Assert.All(
                    expectedOrganisations,
                    (expectedOrganisation, index) =>
                    {
                        var actualOrganisation = result.PublishingOrganisations[index];
                        Assert.Equal(expectedOrganisation.Id, actualOrganisation.Id);
                        Assert.Equal(expectedOrganisation.Title, actualOrganisation.Title);
                        Assert.Equal(expectedOrganisation.Url, actualOrganisation.Url);
                    }
                );
            }
        }

        [Fact]
        public async Task WhenNoUpdatesExist_ReturnsUpdateCountForFirstPublishedEntry()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
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
                var outcome = await sut.GetReleaseVersionSummary(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug
                );

                // Assert
                var result = outcome.AssertRight();
                Assert.Equal(1, result.UpdateCount);
            }
        }
    }

    private static ReleaseVersionsService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
