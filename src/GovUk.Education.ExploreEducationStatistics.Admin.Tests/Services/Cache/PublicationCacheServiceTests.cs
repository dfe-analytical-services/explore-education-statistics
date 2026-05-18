#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using static Moq.MockBehavior;
using Capture = Moq.Capture;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Cache;

public abstract class PublicationCacheServiceTests
{
    public class RemovePublicationTests : PublicationCacheServiceTests
    {
        [Fact]
        public async Task WhenPublicationSlugIsProvided_DeletesBlobsWithExpectedNamePrefixAndOptions()
        {
            // Arrange
            const string publicationSlug = "publication-slug";
            const string? expectedNamePrefixToDelete = null;
            const string expectedIncludeRegex = $"^publications/{publicationSlug}/.+$";

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

            publicBlobStorageService
                .Setup(s =>
                    s.DeleteBlobs(
                        BlobContainers.PublicContent,
                        expectedNamePrefixToDelete,
                        It.Is<IBlobStorageService.DeleteBlobsOptions>(opts =>
                            opts.ExcludeRegex == null
                            && opts.IncludeRegex != null
                            && opts.IncludeRegex.ToString() == expectedIncludeRegex
                        )
                    )
                )
                .Returns(Task.CompletedTask);

            var sut = BuildService(publicBlobStorageService.Object);

            // Act
            await sut.RemovePublication(publicationSlug);

            // Assert
            publicBlobStorageService.VerifyAll();
        }

        [Theory]
        [InlineData("publications/publication-1/something", true)]
        [InlineData("publications/publication-1/releases/something", true)]
        [InlineData("publications/publication-1/releases/release-1/something", true)]
        [InlineData("publications/publication-1", false)] // no trailing path segment and blob name matches publication-slug
        [InlineData("publications/publication-2/releases/something", false)] // does not begin with publications/publication-1/
        [InlineData("something/publication-1/releases/something", false)] // does not begin with publications/publication-1/
        [InlineData("publication-1/something", false)] // does not begin with publications/publication-1/
        public async Task WhenIncludeRegexIsApplied_RegexOnlyMatchesBlobNamesUnderPublicationSlug(
            string blobName,
            bool shouldMatch
        )
        {
            // Arrange
            const string publicationSlug = "publication-1";
            const string? expectedNamePrefixToDelete = null;
            IBlobStorageService.DeleteBlobsOptions options = null!;
            var match = new CaptureMatch<IBlobStorageService.DeleteBlobsOptions>(param => options = param);

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

            publicBlobStorageService
                .Setup(s =>
                    s.DeleteBlobs(BlobContainers.PublicContent, expectedNamePrefixToDelete, Capture.With(match))
                )
                .Returns(Task.CompletedTask);

            // Act
            var sut = BuildService(publicBlobStorageService.Object);

            await sut.RemovePublication(publicationSlug);

            // Assert
            var regex = Assert.IsType<Regex>(options.IncludeRegex);
            Assert.Equal(shouldMatch, regex.IsMatch(blobName));
            publicBlobStorageService.VerifyAll();
        }
    }

    private static PublicationCacheService BuildService(IPublicBlobStorageService? publicBlobStorageService = null) =>
        new(publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict));
}
