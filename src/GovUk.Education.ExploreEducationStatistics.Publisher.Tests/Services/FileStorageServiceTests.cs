using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class FileStorageServiceTests
    {
        [Fact]
        public async Task GetPublicFileInfo()
        {
            const string publicationSlug = "publication-slug";
            const string releaseSlug = "release-slug";

            var file = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "data.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.Setup(s =>
                    s.CheckBlobExists(PublicFilesContainerName, file.PublicPath(publicationSlug, releaseSlug)))
                .ReturnsAsync(true);

            publicBlobStorageService.Setup(s =>
                    s.GetBlob(PublicFilesContainerName, file.PublicPath(publicationSlug, releaseSlug)))
                .ReturnsAsync(new BlobInfo
                (
                    path: file.PublicPath(publicationSlug, releaseSlug),
                    size: "400 B",
                    contentType: "text/csv",
                    contentLength: 400L,
                    meta: GetDataFileMetaValues(
                        name: "Test data file",
                        metaFileName: "test-data.meta.csv",
                        userName: "test@test.com",
                        numberOfRows: 200
                    )
                ));

            var service = BuildFileStorageService(
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.GetPublicFileInfo(publicationSlug, releaseSlug, file);

            publicBlobStorageService.VerifyAll();

            Assert.Equal(file.Id, result.Id);
            Assert.Equal("csv", result.Extension);
            Assert.Equal("data.csv", result.FileName);
            Assert.Equal("Test data file", result.Name);
            Assert.Equal(file.PublicPath(publicationSlug, releaseSlug), result.Path);
            Assert.Equal("400 B", result.Size);
            Assert.Equal(ReleaseFileTypes.Data, result.Type);
        }

        [Fact]
        public async Task GetPublicFileInfo_FileNotFound()
        {
            const string publicationSlug = "publication-slug";
            const string releaseSlug = "release-slug";

            var file = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "data.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.Setup(s =>
                    s.CheckBlobExists(PublicFilesContainerName, file.PublicPath(publicationSlug, releaseSlug)))
                .ReturnsAsync(false);

            var service = BuildFileStorageService(
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.GetPublicFileInfo(publicationSlug, releaseSlug, file);

            publicBlobStorageService.VerifyAll();

            Assert.Equal(file.Id, result.Id);
            Assert.Equal("csv", result.Extension);
            Assert.Equal("data.csv", result.FileName);
            Assert.Equal("Unknown", result.Name);
            Assert.Null(result.Path);
            Assert.Equal("0.00 B", result.Size);
            Assert.Equal(ReleaseFileTypes.Data, result.Type);
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