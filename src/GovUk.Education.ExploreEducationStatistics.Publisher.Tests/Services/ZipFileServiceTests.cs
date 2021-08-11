using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using ICSharpCode.SharpZipLib.Zip;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class ZipFileServiceTests
    {
        [Fact]
        public async Task UploadZippedFiles()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublishScheduled = DateTime.UtcNow
            };

            var files = new List<File>
            {
                new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                },
                new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data
                }
            };

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            foreach (var file in files)
            {
                publicBlobStorageService.Setup(mock => mock.DownloadToStream(
                        PublicReleaseFiles,
                        file.PublicPath(release.Id),
                        It.IsAny<ZipOutputStream>(),
                        null))
                    .ReturnsAsync(new ZipOutputStream(new MemoryStream()));
            }

            publicBlobStorageService.Setup(mock => mock.UploadStream(
                    PublicReleaseFiles,
                    "destination/path/test.zip",
                    It.IsAny<MemoryStream>(),
                    "application/x-zip-compressed",
                    It.Is<IDictionary<string, string>>(metadata =>
                        metadata[BlobInfoExtensions.ReleaseDateTimeKey] ==
                        release.PublishScheduled.Value.ToString("o", CultureInfo.InvariantCulture))))
                .Returns(Task.CompletedTask);

            var service = SetupZipFileService(publicBlobStorageService: publicBlobStorageService.Object);

            await service.UploadZippedFiles(
                containerName: PublicReleaseFiles,
                destinationPath: "destination/path/test.zip",
                files: files,
                releaseId: release.Id,
                publishScheduled: release.PublishScheduled.Value);

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        private static ZipFileService SetupZipFileService(
            IBlobStorageService publicBlobStorageService = null)
        {
            return new ZipFileService(
                publicBlobStorageService ?? new Mock<IBlobStorageService>().Object
            );
        }
    }
}
