#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseFileServiceTests : IDisposable
    {
        private readonly List<string> _filePaths = new();

        public void Dispose()
        {
            // Cleanup any files that have been
            // written to the filesystem.
            _filePaths.ForEach(File.Delete);
        }

        [Fact]
        public async Task StreamFile()
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
                File = new Model.File
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
                size: "100 KB",
                contentType: "application/pdf",
                contentLength: 0L);

            blobStorageService
                .SetupFindBlob(PublicReleaseFiles, releaseFile.PublicPath(), blob);
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile.PublicPath(), "Test blob");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamFile(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
                Assert.Equal("Test blob", result.Right.FileStream.ReadToEnd());
            }
        }

        [Fact]
        public async Task StreamFile_ReleaseNotFound()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                File = new Model.File
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

                var result = await service.StreamFile(Guid.NewGuid(), releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task StreamFile_ReleaseFileNotFound()
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

                var result = await service.StreamFile(release.Id, Guid.NewGuid());

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task StreamFile_BlobDoesNotExist()
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
                File = new Model.File
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

            blobStorageService.SetupFindBlob(PublicReleaseFiles, releaseFile.PublicPath(), null);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamFile(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task StreamAllFilesZip()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var path = release.AllFilesZipPath();

            var blob = new BlobInfo(
                path: release.AllFilesZipPath(),
                size: "100 KB",
                contentType: "application/zip",
                contentLength: 0L);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupFindBlob(PublicReleaseFiles, path, blob);
            blobStorageService.SetupDownloadToStream(PublicReleaseFiles, path, "Test blob");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamAllFilesZip(release.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                Assert.Equal("application/zip", result.Right.ContentType);
                Assert.Equal("publication-slug_release-slug.zip", result.Right.FileDownloadName);
                Assert.Equal("Test blob", result.Right.FileStream.ReadToEnd());
            }
        }

        [Fact]
        public async Task StreamAllFilesZip_ReleaseNotFound()
        {
            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.StreamAllFilesZip(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task StreamAllFilesZip_BlobDoesNotExist()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var path = release.AllFilesZipPath();

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupFindBlob(PublicReleaseFiles, path, null);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.StreamAllFilesZip(release.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ZipFilesToStream_ValidFileTypes()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), true);
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test data blob");
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile2.PublicPath(), "Test ancillary blob");

            var subjectIds = releaseFiles
                .Where(rf => rf.File.SubjectId.HasValue)
                .Select(rf => rf.File.SubjectId.GetValueOrDefault())
                .ToList();

            var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(MockBehavior.Strict);

            dataGuidanceFileWriter
                .Setup(
                    s => s.WriteToStream(
                        It.IsAny<Stream>(),
                        It.Is<Release>(r => r.Id == release.Id),
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.All(id => subjectIds.Contains(id))
                        )
                    )
                )
                .Returns<Stream, Release, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
                .Callback<Stream, Release, IEnumerable<Guid>?>(
                    (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
                );

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var path = GenerateZipFilePath();
                var stream = File.OpenWrite(path);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService, dataGuidanceFileWriter);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("ancillary/ancillary.pdf", zip.Entries[0].FullName);
                Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("data/data.csv", zip.Entries[1].FullName);
                Assert.Equal("Test data blob", zip.Entries[1].Open().ReadToEnd());

                // Data guidance is generated if there is at least one data file
                Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
                Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_DataGuidanceForMultipleDataFiles()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data-1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data-2.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), true);
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test data 1 blob");
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile2.PublicPath(), "Test data 2 blob");

            var subjectIds = releaseFiles
                .Where(rf => rf.File.SubjectId.HasValue)
                .Select(rf => rf.File.SubjectId.GetValueOrDefault())
                .ToList();

            var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(MockBehavior.Strict);

            dataGuidanceFileWriter
                .Setup(
                    s => s.WriteToStream(
                        It.IsAny<Stream>(),
                        It.Is<Release>(r => r.Id == release.Id),
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.All(id => subjectIds.Contains(id))
                        )
                    )
                )
                .Returns<Stream, Release, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
                .Callback<Stream, Release, IEnumerable<Guid>?>(
                    (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
                );

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var path = GenerateZipFilePath();
                var stream = File.OpenWrite(path);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService, dataGuidanceFileWriter);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("data/data-1.csv", zip.Entries[0].FullName);
                Assert.Equal("Test data 1 blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("data/data-2.csv", zip.Entries[1].FullName);
                Assert.Equal("Test data 2 blob", zip.Entries[1].Open().ReadToEnd());

                // Data guidance is generated if there is at least one data file
                Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
                Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_OrderedAlphabetically()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-2.pdf",
                    Type = Ancillary,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-3.pdf",
                    Type = Ancillary
                }
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-1.pdf",
                    Type = Ancillary
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), true);
            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile3.PublicPath(), true);
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile1.PublicPath(), "Test 2 blob");
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile2.PublicPath(), "Test 3 blob");;
            blobStorageService
                .SetupDownloadToStream(PublicReleaseFiles, releaseFile3.PublicPath(), "Test 1 blob");;

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("ancillary/test-1.pdf", zip.Entries[0].FullName);
                Assert.Equal("Test 1 blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("ancillary/test-2.pdf", zip.Entries[1].FullName);
                Assert.Equal("Test 2 blob", zip.Entries[1].Open().ReadToEnd());

                Assert.Equal("ancillary/test-3.pdf", zip.Entries[2].FullName);
                Assert.Equal("Test 3 blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersInvalidFileTypes()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.meta.csv",
                    Type = Metadata,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.zip",
                    Type = DataZip
                }
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.jpg",
                    Type = Chart
                }
            };
            var releaseFile4 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.jpg",
                    Type = Image
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3, releaseFile4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersFilesNotInBlobStorage()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.pdf",
                    Type = FileType.Data,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            // Files do not exist in blob storage
            blobStorageService.SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), false);
            blobStorageService.SetupCheckBlobExists(PublicReleaseFiles, releaseFile2.PublicPath(), false);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersFilesForOtherReleases()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            // Files are for other releases
            var releaseFile1 = new ReleaseFile
            {
                Release = new Release(),
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-1.pdf",
                    Type = Ancillary,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = new Release(),
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-2.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_Empty()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
                var result = await service.ZipFilesToStream(release.Id, fileIds, stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_Cancelled()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-1.pdf",
                    Type = Ancillary
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new Model.File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-2.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = File.OpenWrite(path);

            var tokenSource = new CancellationTokenSource();

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            // After the first file has completed, we cancel the request
            // to prevent the next file from being fetched.
            blobStorageService
                .SetupCheckBlobExists(PublicReleaseFiles, releaseFile1.PublicPath(), true);
            blobStorageService
                .SetupDownloadToStream(
                    container: PublicReleaseFiles,
                    path: releaseFile1.PublicPath(),
                    blobText: "Test ancillary blob",
                    cancellationToken: tokenSource.Token)
                .Callback(() => tokenSource.Cancel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    fileIds: fileIds,
                    outputStream: stream,
                    cancellationToken: tokenSource.Token);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Single(zip.Entries);
                Assert.Equal("ancillary/ancillary-1.pdf", zip.Entries[0].FullName);
                Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());
            }
        }

        private string GenerateZipFilePath()
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".zip";
            _filePaths.Add(path);

            return path;
        }

        private static ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IUserService? userService = null)
        {
            return new (
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
