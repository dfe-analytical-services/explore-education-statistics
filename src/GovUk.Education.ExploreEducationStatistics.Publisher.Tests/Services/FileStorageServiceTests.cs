using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class FileStorageServiceTests
    {
        [Fact]
        public async Task CheckBlobExists_BlobExists()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.Setup(s =>
                    s.CheckBlobExists(PublicReleaseFiles, "path"))
                .ReturnsAsync(true);

            var service = BuildFileStorageService(
                publicBlobStorageService: publicBlobStorageService.Object);

            Assert.True(await service.CheckBlobExists(PublicReleaseFiles, "path"));
        }

        [Fact]
        public async Task CheckBlobExists_BlobDoesNotExist()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.Setup(s =>
                    s.CheckBlobExists(PublicReleaseFiles, "path"))
                .ReturnsAsync(false);

            var service = BuildFileStorageService(
                publicBlobStorageService: publicBlobStorageService.Object);

            Assert.False(await service.CheckBlobExists(PublicReleaseFiles, "path"));
        }

        [Fact]
        public async Task GetBlob()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blobInfo = new BlobInfo
            (
                path: "data.csv",
                size: "400 B",
                contentType: "text/csv",
                contentLength: 0L,
                meta: FileStorageUtils.GetAncillaryFileMetaValues(
                    name: "Test data file"
                )
            );

            publicBlobStorageService.Setup(s =>
                    s.GetBlob(PublicReleaseFiles, "path"))
                .ReturnsAsync(blobInfo);

            var service = BuildFileStorageService(
                publicBlobStorageService: publicBlobStorageService.Object);

            Assert.Equal(blobInfo, await service.GetBlob(PublicReleaseFiles, "path"));
        }

        private static FileStorageService BuildFileStorageService(
            IBlobStorageService privateBlobStorageService = null,
            IBlobStorageService publicBlobStorageService = null,
            string publicStorageConnectionString = null,
            string publisherStorageConnectionString = null)
        {
            return new FileStorageService(
                privateBlobStorageService ?? new Mock<IBlobStorageService>().Object,
                publicBlobStorageService ?? new Mock<IBlobStorageService>().Object,
                publicStorageConnectionString,
                publisherStorageConnectionString,
                new Mock<ILogger<FileStorageService>>().Object);
        }
    }
}
