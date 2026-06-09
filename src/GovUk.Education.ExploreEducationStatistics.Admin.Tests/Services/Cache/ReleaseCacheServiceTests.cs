#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Cache;

public abstract class ReleaseCacheServiceTests
{
    public class RemoveReleaseTests : ReleaseCacheServiceTests
    {
        [Fact]
        public async Task WhenPublicationAndReleaseSlugIsProvided_DeletesBlobsWithExpectedNamePrefixAndOptions()
        {
            // Arrange
            const string publicationSlug = "publication-slug";
            const string releaseSlug = "release-slug";
            const string expectedNamePrefixToDelete = $"publications/{publicationSlug}/releases/{releaseSlug}/";
            IBlobStorageService.DeleteBlobsOptions? expectedOptions = null;

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

            publicBlobStorageService
                .Setup(s => s.DeleteBlobs(PublicContent, expectedNamePrefixToDelete, expectedOptions))
                .Returns(Task.CompletedTask);

            var sut = BuildService(publicBlobStorageService.Object);

            // Act
            await sut.RemoveRelease(publicationSlug, releaseSlug);

            // Assert
            publicBlobStorageService.VerifyAll();
        }
    }

    private static ReleaseCacheService BuildService(IPublicBlobStorageService? publicBlobStorageService = null) =>
        new(publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict));
}
