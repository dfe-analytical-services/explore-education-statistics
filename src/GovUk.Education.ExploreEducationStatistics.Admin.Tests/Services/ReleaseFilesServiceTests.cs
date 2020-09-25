using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServiceTests
    {
        [Fact]
        public async void GetDataFile()
        {
            var release = new Release();
            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };
            var metaFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFileReference
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var blobPath = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, blobPath))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, blobPath))
                    .ReturnsAsync(
                        new BlobInfo(
                            blobPath,
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            meta: GetDataFileMetaValues(
                                name: "Test data file name",
                                metaFileName: "test-data.meta.csv",
                                userName: "test@test.com",
                                numberOfRows: 200
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseFilesService(context, blobStorageService.Object);
                var result = await service.GetDataFile(
                    release.Id,
                    dataFileReference.Id
                );

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var fileInfo = result.Right;

                Assert.Equal(dataFileReference.Id, fileInfo.Id);
                Assert.Equal("Test data file name", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(blobPath, fileInfo.Path);
                Assert.Equal(metaFileReference.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(DateTimeOffset.Parse("2020-09-16T12:00:00Z"), fileInfo.Created);
            }
        }

        [Fact]
        public async void GetDataFile_ReleaseNotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // A file for another release exists
                var anotherRelease = new Release();
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = anotherRelease,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = anotherRelease,
                            Filename = "test-data.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata
                        }
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFilesService(context);
                var result = await service.GetDataFile(
                    Guid.NewGuid(),
                    Guid.NewGuid()
                );

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async void GetDataFile_FileNotFound()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(
                    // Not for this release
                    new ReleaseFile
                    {
                        Release = new Release(),
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "test-data.csv",
                            ReleaseFileType = ReleaseFileTypes.Data
                        }
                    },
                    // Not the right type
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "ancillary-file.pdf",
                            ReleaseFileType = ReleaseFileTypes.Ancillary
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFilesService(context);
                var result = await service.GetDataFile(
                    release.Id,
                    Guid.NewGuid()
                );

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async void GetDataFile_NoMatchingBlob()
        {
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject name"
            };
            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };
            var metaFileReference = new ReleaseFileReference
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFileReference
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var blobPath = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, blobPath))
                    .ReturnsAsync(false);

                var subjectService = new Mock<ISubjectService>();

                subjectService
                    .Setup(s => s.GetAsync(subject.Id))
                    .ReturnsAsync(subject);

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    subjectService: subjectService.Object
                );
                var result = await service.GetDataFile(
                    release.Id,
                    dataFileReference.Id
                );

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var fileInfo = result.Right;

                Assert.Equal(dataFileReference.Id, fileInfo.Id);
                Assert.Equal("Test subject name", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal("test-data.csv", fileInfo.Path);
                Assert.Equal(metaFileReference.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("", fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal("0.00 B", fileInfo.Size);
            }
        }

        [Fact]
        public async void GetDataFile_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-archive.zip",
                ReleaseFileType = ReleaseFileTypes.DataZip,
            };
            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Source = zipFileReference,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = zipFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "test-data.meta.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata,
                            Source = zipFileReference
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var dataBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data.csv");
                var zipBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.DataZip, "test-data-archive.zip");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataBlobPath))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipBlobPath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                name: "Test data",
                                metaFileName: "test-data.meta.csv",
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseFilesService(context, blobStorageService.Object);
                var result = await service.GetDataFile(
                    release.Id,
                    dataFileReference.Id
                );

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var fileInfo = result.Right;

                Assert.Equal(dataFileReference.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal("test-data.csv", fileInfo.Path);
                Assert.False(fileInfo.MetaFileId.HasValue);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal("1 Mb", fileInfo.Size);
            }
        }

        [Fact]
        public async void ListDataFiles()
        {
            var release = new Release();
            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject name"
            };
            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject name"
            };
            var contextId = Guid.NewGuid().ToString();

            var dataFile1Reference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-1.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };
            var metaFile1Reference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-1.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject1.Id
            };
            var dataFile2Reference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-2.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id
            };
            var metaFile2Reference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-2.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject2.Id
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFile1Reference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFile1Reference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFile2Reference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFile2Reference
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var dataFile1Path = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data-1.csv");
                var dataFile2Path = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data-2.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile1Path))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile1Path))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile1Path,
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            GetDataFileMetaValues(
                                name: "Test data file 1",
                                metaFileName: "test-data-1.meta.csv",
                                userName: "test1@test.com",
                                numberOfRows: 200
                            )
                        )
                    );

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile2Path))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile2Path))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile2Path,
                            size: "800 B",
                            contentType: "text/csv",
                            contentLength: 800L,
                            GetDataFileMetaValues(
                                name: "Test data file 2",
                                metaFileName: "test-data-2.meta.csv",
                                userName: "test2@test.com",
                                numberOfRows: 400
                            )
                        )
                    );

                var service = SetupReleaseFilesService(context, blobStorageService.Object);
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Equal(2, files.Count);

                Assert.Equal(dataFile1Reference.Id, files[0].Id);
                Assert.Equal("Test data file 1", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile1Path, files[0].Path);
                Assert.Equal(metaFile1Reference.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal("test1@test.com", files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("400 B", files[0].Size);

                Assert.Equal(dataFile2Reference.Id, files[1].Id);
                Assert.Equal("Test data file 2", files[1].Name);
                Assert.Equal("test-data-2.csv", files[1].FileName);
                Assert.Equal("csv", files[1].Extension);
                Assert.Equal(dataFile2Path, files[1].Path);
                Assert.Equal(metaFile2Reference.Id, files[1].MetaFileId);
                Assert.Equal("test-data-2.meta.csv", files[1].MetaFileName);
                Assert.Equal("test2@test.com", files[1].UserName);
                Assert.Equal(400, files[1].Rows);
                Assert.Equal("800 B", files[1].Size);
            }
        }

        [Fact]
        public async void ListDataFiles_FiltersCorrectly()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-1.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };
            var metaFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-1.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var otherRelease = new Release();

                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFileReference
                    },
                    // For other release
                    new ReleaseFile
                    {
                        Release = otherRelease,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = otherRelease,
                            Filename = "test-data-2.csv",
                            ReleaseFileType = ReleaseFileTypes.Data
                        }
                    },
                    new ReleaseFile
                    {
                        Release = otherRelease,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = otherRelease,
                            Filename = "test-data-2.meta.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata
                        }
                    },
                    // Not the right type
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "ancillary-file.pdf",
                            ReleaseFileType = ReleaseFileTypes.Ancillary
                        }
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var dataFile1Path = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data-1.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile1Path))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile1Path))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile1Path,
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            GetDataFileMetaValues(
                                name: "Test data file 1",
                                metaFileName: "test-data-1.meta.csv",
                                userName: "test1@test.com",
                                numberOfRows: 200
                            )
                        )
                    );

                var service = SetupReleaseFilesService(context, blobStorageService.Object);
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFileReference.Id, files[0].Id);
                Assert.Equal("Test data file 1", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile1Path, files[0].Path);
                Assert.Equal(metaFileReference.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal("test1@test.com", files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("400 B", files[0].Size);
            }
        }

        [Fact]
        public async void ListDataFiles_NoMatchingBlob()
        {
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject name"
            };
            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data
            };
            var metaFileReference = new ReleaseFileReference
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = metaFileReference
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var blobPath = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, blobPath))
                    .ReturnsAsync(false);

                var subjectService = new Mock<ISubjectService>();

                subjectService
                    .Setup(s => s.GetAsync(subject.Id))
                    .ReturnsAsync(subject);

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    subjectService: subjectService.Object
                );
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);
                Assert.Equal(dataFileReference.Id, files[0].Id);
                Assert.Equal("Test subject name", files[0].Name);
                Assert.Equal("test-data.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal("test-data.csv", files[0].Path);
                Assert.Equal(metaFileReference.Id, files[0].MetaFileId);
                Assert.Equal("test-data.meta.csv", files[0].MetaFileName);
                Assert.Equal("", files[0].UserName);
                Assert.Equal(0, files[0].Rows);
                Assert.Equal("0.00 B", files[0].Size);
            }
        }

        [Fact]
        public async void ListDataFiles_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data-archive.zip",
                ReleaseFileType = ReleaseFileTypes.DataZip,
            };
            var dataFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Source = zipFileReference,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = zipFileReference
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "test-data.meta.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata,
                            Source = zipFileReference
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var dataBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.Data, "test-data.csv");
                var zipBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.DataZip, "test-data-archive.zip");

                var blobStorageService = new Mock<IBlobStorageService>();

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataBlobPath))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipBlobPath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                name: "Test data",
                                metaFileName: "test-data.meta.csv",
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseFilesService(context, blobStorageService.Object);
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFileReference.Id, files[0].Id);
                Assert.Equal("Test data", files[0].Name);
                Assert.Equal("test-data.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal("test-data.csv", files[0].Path);
                Assert.False(files[0].MetaFileId.HasValue);
                Assert.Equal("test-data.meta.csv", files[0].MetaFileName);
                Assert.Equal("test@test.com", files[0].UserName);
                Assert.Equal(0, files[0].Rows);
                Assert.Equal("1 Mb", files[0].Size);
            }
        }

        public ReleaseFilesService SetupReleaseFilesService(
            ContentDbContext context,
            IBlobStorageService blobStorageService = null,
            IUserService userService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IImportService importService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            ISubjectService subjectService = null,
            IDataArchiveValidationService dataArchiveValidationService = null)
        {
            return new ReleaseFilesService(
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                context,
                importService ?? new Mock<IImportService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>().Object
            );
        }
    }
}