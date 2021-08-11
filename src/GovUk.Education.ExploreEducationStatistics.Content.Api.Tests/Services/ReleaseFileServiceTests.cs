#nullable enable
using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;
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
                        It.IsAny<MemoryStream>(), null))
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
                    mock =>
                        mock.DownloadToStream(
                            PublicReleaseFiles, releaseFile.PublicPath(),
                        It.IsAny<MemoryStream>(), null), Times.Once());

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
                        It.IsAny<MemoryStream>(), null))
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
                        It.IsAny<MemoryStream>(), null), Times.Once());

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

        [Fact]
        public async Task ListDownloadFiles()
        {
            var release = new Release()
            {
                ReleaseName = "2020",
                Slug = "2020",
                Publication = new Publication
                {
                    Slug = "test-publication"
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Test data 1",
                Release = release,
                Summary = "Test data 1 summary",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test-data-1.csv",
                    Created = DateTime.Now,
                    CreatedBy = new User
                    {
                        Email = "user1@test.com"
                    }
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Name = "Test ancillary 1",
                Release = release,
                Summary = "Test ancillary 1 summary",
                File = new File
                {
                    Type = FileType.Ancillary,
                    Filename = "test-ancillary-1.pdf",
                    Created = DateTime.Now,
                    CreatedBy = new User
                    {
                        Email = "user2@test.com"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var dataFilePath = releaseFile1.File.PublicPath(release.Id);
                var ancillaryFilePath = releaseFile2.File.PublicPath(release.Id);
                var allFilesZipPath = release.AllFilesZipPath();

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFilePath,
                            size: "9.9 MB",
                            contentType: "text/csv",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, ancillaryFilePath)
                    )
                    .ReturnsAsync(true);

                blobStorageService.Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, ancillaryFilePath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: ancillaryFilePath,
                            size: "100 KB",
                            contentType: "application/pdf",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(true);

                blobStorageService.Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: allFilesZipPath,
                            size: "5 MB",
                            contentType: "application/zip",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.ListDownloadFiles(release);

                Assert.Equal(3, result.Count);
                Assert.Equal("All files", result[0].Name);
                Assert.Equal("test-publication_2020.zip", result[0].FileName);
                Assert.Equal("5 MB", result[0].Size);
                Assert.Equal(FileType.Ancillary, result[0].Type);
                Assert.Null(result[0].Created);
                Assert.Null(result[0].UserName);

                Assert.Equal("Test ancillary 1", result[1].Name);
                Assert.Equal("test-ancillary-1.pdf", result[1].FileName);
                Assert.Equal("Test ancillary 1 summary", result[1].Summary);
                Assert.Equal("100 KB", result[1].Size);
                Assert.Equal(FileType.Ancillary, result[1].Type);
                Assert.Null(result[1].Created);
                Assert.Null(result[1].UserName);

                Assert.Equal("Test data 1", result[2].Name);
                Assert.Equal("test-data-1.csv", result[2].FileName);
                Assert.Equal("Test data 1 summary", result[2].Summary);
                Assert.Equal("9.9 MB", result[2].Size);
                Assert.Equal(FileType.Data, result[2].Type);
                Assert.Null(result[2].Created);
                Assert.Null(result[2].UserName);

                MockUtils.VerifyAllMocks(blobStorageService);
            }
        }

        [Fact]
        public async Task ListDownloadFiles_FiltersOutInvalidFileTypes()
        {
            var release = new Release()
            {
                ReleaseName = "2020",
                Slug = "2020",
                Publication = new Publication
                {
                    Slug = "test-publication"
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Test data 1",
                Release = release,
                Summary = "Test data 1 summary",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test-data-1.csv",
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Type = FileType.Chart,
                    Filename = "test-chart-1.jpg",
                }
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Type = FileType.Image,
                    Filename = "test-image-1.jpg",
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var dataFilePath = releaseFile1.File.PublicPath(release.Id);
                var allFilesZipPath = release.AllFilesZipPath();

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFilePath,
                            size: "10 MB",
                            contentType: "text/csv",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(true);

                blobStorageService.Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: allFilesZipPath,
                            size: "5 MB",
                            contentType: "application/zip",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.ListDownloadFiles(release);

                Assert.Equal(2, result.Count);
                Assert.Equal("All files", result[0].Name);
                Assert.Equal("test-publication_2020.zip", result[0].FileName);
                Assert.Equal("5 MB", result[0].Size);
                Assert.Equal(FileType.Ancillary, result[0].Type);
                Assert.Null(result[0].Created);
                Assert.Null(result[0].UserName);

                Assert.Equal("Test data 1", result[1].Name);
                Assert.Equal("test-data-1.csv", result[1].FileName);
                Assert.Equal("Test data 1 summary", result[1].Summary);
                Assert.Equal("10 MB", result[1].Size);
                Assert.Equal(FileType.Data, result[1].Type);
                Assert.Null(result[1].Created);
                Assert.Null(result[1].UserName);
            }
        }

        [Fact]
        public async Task ListDownloadFiles_MissingFileBlob()
        {
            var release = new Release()
            {
                ReleaseName = "2020",
                Slug = "2020",
                Publication = new Publication
                {
                    Slug = "test-publication"
                }
            };
            var releaseFile1 = new ReleaseFile
            {
                Name = "Test data 1",
                Release = release,
                Summary = "Test data 1 summary",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test-data-1.csv",
                    Created = DateTime.Now,
                    CreatedBy = new User
                    {
                        Email = "user1@test.com"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var dataFilePath = releaseFile1.File.PublicPath(release.Id);
                var allFilesZipPath = release.AllFilesZipPath();

                // File blob is missing
                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(false);

                // All files zip can still be fetched as the
                // missing file may be in there instead.
                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(true);

                blobStorageService.Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: allFilesZipPath,
                            size: "5 MB",
                            contentType: "application/zip",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.ListDownloadFiles(release);

                Assert.Equal(2, result.Count);

                Assert.Equal("All files", result[0].Name);
                Assert.Equal("test-publication_2020.zip", result[0].FileName);
                Assert.Equal("5 MB", result[0].Size);
                Assert.Equal(FileType.Ancillary, result[0].Type);
                Assert.Null(result[0].Created);
                Assert.Null(result[0].UserName);

                Assert.Equal("Test data 1", result[1].Name);
                Assert.Equal("test-data-1.csv", result[1].FileName);
                Assert.Equal("Test data 1 summary", result[1].Summary);
                Assert.Equal("0.00 B", result[1].Size);
                Assert.Equal(FileType.Data, result[1].Type);
                Assert.Null(result[0].Created);
                Assert.Null(result[0].UserName);

                MockUtils.VerifyAllMocks(blobStorageService);
            }
        }

        [Fact]
        public async Task ListDownloadFiles_MissingAllFilesBlob()
        {
            var release = new Release()
            {
                ReleaseName = "2020",
                Slug = "2020",
                Publication = new Publication
                {
                    Slug = "test-publication"
                }
            };
            var releaseFile1 = new ReleaseFile
            {
                Name = "Test data 1",
                Release = release,
                Summary = "Test data 1 summary",
                File = new File
                {
                    Type = FileType.Data,
                    Filename = "test-data-1.csv",
                    Created = DateTime.Now,
                    CreatedBy = new User
                    {
                        Email = "user1@test.com"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var dataFilePath = releaseFile1.File.PublicPath(release.Id);
                var allFilesZipPath = release.AllFilesZipPath();

                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(
                        mock =>
                            mock.GetBlob(PublicReleaseFiles, dataFilePath)
                    )
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFilePath,
                            size: "9.9 MB",
                            contentType: "text/csv",
                            contentLength: 0L,
                            meta: new Dictionary<string, string>()
                        )
                    );

                // All files blob is missing
                blobStorageService.Setup(
                        mock =>
                            mock.CheckBlobExists(PublicReleaseFiles, allFilesZipPath)
                    )
                    .ReturnsAsync(false);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.ListDownloadFiles(release);

                Assert.Equal(2, result.Count);

                Assert.Equal("All files", result[0].Name);
                Assert.Equal("test-publication_2020.zip", result[0].FileName);
                Assert.Equal("0.00 B", result[0].Size);
                Assert.Equal(FileType.Ancillary, result[0].Type);
                Assert.Null(result[0].Created);
                Assert.Null(result[0].UserName);

                Assert.Equal("Test data 1", result[1].Name);
                Assert.Equal("test-data-1.csv", result[1].FileName);
                Assert.Equal("Test data 1 summary", result[1].Summary);
                Assert.Equal("9.9 MB", result[1].Size);
                Assert.Equal(FileType.Data, result[1].Type);
                Assert.Null(result[1].Created);
                Assert.Null(result[1].UserName);

                MockUtils.VerifyAllMocks(blobStorageService);
            }
        }

        [Fact]
        public async Task ListDownloadFiles_NoFiles()
        {
            var release = new Release()
            {
                ReleaseName = "2020",
                Slug = "2020",
                Publication = new Publication
                {
                    Slug = "test-publication"
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.ListDownloadFiles(release);

                Assert.Empty(result);

                MockUtils.VerifyAllMocks(blobStorageService);
            }
        }

        private static ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null)
        {
            return new (
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                Mock.Of<ILogger<ReleaseFileService>>()
            );
        }
    }
}
