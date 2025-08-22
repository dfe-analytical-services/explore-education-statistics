using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public abstract class RelatedInformationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetAllRelatedInformationForReleaseTests : RelatedInformationServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task GetAllRelatedInformationForRelease_ReturnsRelatedInformation_WhenPublicationAndReleaseExist(
            int numRelatedInformation)
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var relatedInformation = _dataFixture.DefaultLink()
                .GenerateArray(numRelatedInformation);
            release.Versions[0].RelatedInformation = [.. relatedInformation];

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
                var outcome = await sut.GetRelatedInformationForRelease(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.Equal(relatedInformation.Length, result.Length);
                Assert.All(relatedInformation,
                    (expected, index) =>
                    {
                        var actual = result[index];
                        Assert.Equal(expected.Id, actual.Id);
                        Assert.Equal(expected.Description, actual.Title);
                        Assert.Equal(expected.Url, actual.Url);
                    });
            }
        }

        [Fact]
        public async Task GetAllRelatedInformationForRelease_ReturnsError_WhenPublicationDoesNotExist()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetRelatedInformationForRelease(
                publicationSlug: publicationSlug,
                releaseSlug: "test-release");

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task GetAllRelatedInformationForRelease_ReturnsError_WhenReleaseDoesNotExist()
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
                var outcome = await sut.GetRelatedInformationForRelease(
                    publicationSlug: publication.Slug,
                    releaseSlug: releaseSlug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetAllRelatedInformationForRelease_ReturnsError_WhenReleaseHasNoPublishedVersion()
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
                var outcome = await sut.GetRelatedInformationForRelease(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static RelatedInformationService BuildService(ContentDbContext contentDbContext)
    {
        return new RelatedInformationService(contentDbContext);
    }
}
