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

public abstract class PublicationReleaseServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPublicationReleasesTests : PublicationReleaseServiceTests
    {
        [Fact]
        public async Task WhenReleaseHasNoPublishedVersions_ReleaseIsExcludedFromResults()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2025),
                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024)
                ]);

            var publishedRelease = publication.Releases.Single(r => r.Year == 2024);

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var releaseEntry = Assert.IsType<PublicationReleaseEntryDto>(pagedResult.Results.Single());

                // Only the published release should be in results
                Assert.Equal(publishedRelease.Id, releaseEntry.ReleaseId);
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
            var outcome = await sut.GetPublicationReleases(publicationSlug);

            // Assert
            outcome.AssertNotFound();
        }
    }

    private static PublicationReleasesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
