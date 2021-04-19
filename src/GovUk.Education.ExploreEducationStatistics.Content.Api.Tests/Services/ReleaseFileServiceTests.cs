using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class ReleaseFileServiceTests
    {
        [Fact]
        public async Task Stream()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: releaseFile.PublicPath(),
                size: null,
                contentType: "application/pdf",
                contentLength: 0L,
                meta: GetMetaValuesReleaseDateTime(
                    releaseDateTime: DateTime.UtcNow.AddDays(-1)),
                created: null);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath()))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PublicReleaseFiles, releaseFile.PublicPath()))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PublicReleaseFiles, releaseFile.PublicPath(),
                        It.IsAny<MemoryStream>()))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath()),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.GetBlob(PublicReleaseFiles, releaseFile.PublicPath()),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.DownloadToStream(PublicReleaseFiles, releaseFile.PublicPath(),
                        It.IsAny<MemoryStream>()), Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_ReleaseNotFound()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(Guid.NewGuid(), releaseFile.File.Id);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_ReleaseFileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_BlobDoesNotExist()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, It.IsAny<string>()))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                result.AssertNotFound();

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath()),
                    Times.Once());
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_BlobIsNotPublished()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: releaseFile.PublicPath(),
                size: null,
                contentType: "application/pdf",
                contentLength: 0L,
                meta: GetMetaValuesReleaseDateTime(
                    releaseDateTime: DateTime.UtcNow.AddDays(1)),
                created: null);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath()))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PublicReleaseFiles, releaseFile.PublicPath()))
                .ReturnsAsync(blob);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                result.AssertNotFound();

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath()),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.GetBlob(PublicReleaseFiles, releaseFile.PublicPath()),
                    Times.Once());
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task StreamByPath()
        {
            const string path = "path/all-files.zip";

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: path,
                size: null,
                contentType: "application/zip",
                contentLength: 0L,
                meta: GetMetaValuesReleaseDateTime(
                    releaseDateTime: DateTime.UtcNow.AddDays(-1)),
                created: null);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, path))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PublicReleaseFiles, path))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PublicReleaseFiles, path,
                        It.IsAny<MemoryStream>()))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamByPath(path);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, path),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.GetBlob(PublicReleaseFiles, path),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.DownloadToStream(PublicReleaseFiles, path,
                        It.IsAny<MemoryStream>()), Times.Once());

                Assert.Equal("application/zip", result.Right.ContentType);
                Assert.Equal("all-files.zip", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task StreamByPath_BlobDoesNotExist()
        {
            const string path = "path/all-files.zip";

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, It.IsAny<string>()))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamByPath(path);

                result.AssertNotFound();

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, path),
                    Times.Once());
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task StreamByPath_BlobIsNotPublished()
        {
            const string path = "path/all-files.zip";

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: path,
                size: null,
                contentType: "application/pdf",
                contentLength: 0L,
                meta: GetMetaValuesReleaseDateTime(
                    releaseDateTime: DateTime.UtcNow.AddDays(1)),
                created: null);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PublicReleaseFiles, path))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PublicReleaseFiles, path))
                .ReturnsAsync(blob);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamByPath(path);

                result.AssertNotFound();

                blobStorageService.Verify(
                    mock => mock.CheckBlobExists(PublicReleaseFiles, path),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.GetBlob(PublicReleaseFiles, path),
                    Times.Once());
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        private static ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null)
        {
            return new ReleaseFileService(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object
            );
        }
    }
}
