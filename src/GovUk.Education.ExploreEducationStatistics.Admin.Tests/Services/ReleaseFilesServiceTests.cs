using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServiceTests
    {
        [Fact]
        public async Task DeleteDataFiles()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var zipFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.zip",
                ReleaseFileType = ReleaseFileTypes.DataZip,
                SubjectId = subject.Id,
            };

            var dataFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = metaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.AddRangeAsync(releaseDataFile, releaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            importService.Setup(mock => mock.RemoveImportTableRowIfExists(release.Id, dataFile.Filename))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock => mock.DeleteBlob(PrivateFilesContainerName, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseFilesService = SetupReleaseFilesService(context: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await releaseFilesService.DeleteDataFiles(release.Id, dataFile.Id);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/data/data.csv"), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/data/data.meta.csv"), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/zip/data.zip"), Times.Once());

                importService.Verify(mock => 
                        mock.RemoveImportTableRowIfExists(release.Id, dataFile.Filename), Times.Once());

                Assert.True(result.IsRight);

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));

                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(dataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(metaFile.Id));
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(zipFile.Id));
            }
        }

        [Fact]
        public async Task DeleteDataFiles_DeleteFilesFromAmendment()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id 
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var zipFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.zip",
                ReleaseFileType = ReleaseFileTypes.DataZip,
                SubjectId = subject.Id,
            };

            var dataFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new ReleaseFileReference
            {
                Release = release,
                Filename = "data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = metaFile
            };

            var amendmentReleaseDataFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = dataFile
            };

            var amendmentReleaseMetaFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = metaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.AddRangeAsync(
                    releaseDataFile, releaseMetaFile, amendmentReleaseDataFile, amendmentReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseFilesService = SetupReleaseFilesService(context: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await releaseFilesService.DeleteDataFiles(amendmentRelease.Id, dataFile.Id);

                Assert.True(result.IsRight);

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseDataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseMetaFile.Id));

                Assert.NotNull(await contentDbContext.ReleaseFileReferences.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFileReferences.FindAsync(metaFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFileReferences.FindAsync(zipFile.Id));
            }
        }

        [Fact]
        public async Task GetDataFile()
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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = IStatus.COMPLETE
                    });

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.GetDataFile(
                    release.Id,
                    dataFileReference.Id
                );

                importStatusService.VerifyAll();
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
                Assert.Equal(IStatus.COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetDataFile_ReleaseNotFound()
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
                Assert.Equal(IStatus.NOT_FOUND, fileInfo.Status);
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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.PROCESSING_ARCHIVE_FILE,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.GetDataFile(
                    release.Id,
                    dataFileReference.Id
                );

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

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
                Assert.Equal(IStatus.PROCESSING_ARCHIVE_FILE, fileInfo.Status);
                Assert.Equal("1 Mb", fileInfo.Size);
            }
        }

        [Fact]
        public async void GetDataFile_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

            var dataFileReference = new ReleaseFileReference
            {
                Release = originalRelease,
                Filename = "test-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
            };
            var metaFileReference = new ReleaseFileReference
            {
                Release = originalRelease,
                Filename = "test-data.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(dataFileReference, metaFileReference);
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = metaFileReference
                    }
                );
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        ReleaseFileReference = dataFileReference
                    },
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        ReleaseFileReference = metaFileReference
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var blobPath = AdminReleasePath(originalRelease.Id, ReleaseFileTypes.Data, "test-data.csv");

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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(originalRelease.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.GetDataFile(
                    amendedRelease.Id,
                    dataFileReference.Id
                );

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

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
                Assert.Equal(IStatus.COMPLETE, fileInfo.Status);
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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data-1.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data-2.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.RUNNING_PHASE_2,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

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
                Assert.Equal(IStatus.COMPLETE, files[0].Status);

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
                Assert.Equal(IStatus.RUNNING_PHASE_2, files[1].Status);
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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data-1.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

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
                Assert.Equal(IStatus.COMPLETE, files[0].Status);
            }
        }

        [Fact]
        public async void ListDataFiles_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

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
                Release = originalRelease,
                Filename = "test-data-1.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };
            var metaFile1Reference = new ReleaseFileReference
            {
                Release = originalRelease,
                Filename = "test-data-1.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject1.Id
            };
            var dataFile2Reference = new ReleaseFileReference
            {
                Release = originalRelease,
                Filename = "test-data-2.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id
            };
            var metaFile2Reference = new ReleaseFileReference
            {
                Release = originalRelease,
                Filename = "test-data-2.meta.csv",
                ReleaseFileType = ReleaseFileTypes.Metadata,
                SubjectId = subject2.Id
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = dataFile1Reference
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = metaFile1Reference
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = dataFile2Reference
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        ReleaseFileReference = metaFile2Reference
                    }
                );
                // Only second data file is attached to this release
                await context.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        ReleaseFileReference = dataFile2Reference
                    },
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        ReleaseFileReference = metaFile2Reference
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var dataFile2Path = AdminReleasePath(originalRelease.Id, ReleaseFileTypes.Data, "test-data-2.csv");

                var blobStorageService = new Mock<IBlobStorageService>();

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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(originalRelease.Id, "test-data-2.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.RUNNING_PHASE_2,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.ListDataFiles(amendedRelease.Id);

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile2Reference.Id, files[0].Id);
                Assert.Equal("Test data file 2", files[0].Name);
                Assert.Equal("test-data-2.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile2Path, files[0].Path);
                Assert.Equal(metaFile2Reference.Id, files[0].MetaFileId);
                Assert.Equal("test-data-2.meta.csv", files[0].MetaFileName);
                Assert.Equal("test2@test.com", files[0].UserName);
                Assert.Equal(400, files[0].Rows);
                 Assert.Equal("800 B", files[0].Size);
                Assert.Equal(IStatus.RUNNING_PHASE_2, files[0].Status);
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
                Assert.Equal(IStatus.NOT_FOUND, files[0].Status);
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

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.PROCESSING_ARCHIVE_FILE,
                        }
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );
                var result = await service.ListDataFiles(release.Id);

                blobStorageService.VerifyAll();
                importStatusService.VerifyAll();

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
                Assert.Equal(IStatus.PROCESSING_ARCHIVE_FILE, files[0].Status);
            }
        }

        [Fact]
        public async void UploadDataFilesAsZip()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);

                await context.SaveChangesAsync();
            }

            var dataFileName = "test-data.csv";
            var metaFileName = "test-data.meta.csv";
            var zipFileName = "test-data-archive.zip";

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var zipFile = CreateZipFormFileMock(zipFileName);
                var archiveFile = CreateDataArchiveFileMock(dataFileName, metaFileName);

                var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>();

                fileUploadsValidatorService
                    .Setup(s => s.ValidateSubjectName(release.Id, "Test data"))
                    .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile.Object))
                    .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

                var dataArchiveValidationService = new Mock<IDataArchiveValidationService>();

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(release.Id, zipFile.Object))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile.Object));

                var importService = new Mock<IImportService>();

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

                var blobStorageService = new Mock<IBlobStorageService>();

                var zipBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.DataZip, zipFileName);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: zipBlobPath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                name: "Test data",
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importService: importService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.UploadDataFilesAsZip(
                    release.Id,
                    zipFile.Object,
                    userName: "test@test.com",
                    subjectName: "Test data");

                fileUploadsValidatorService.VerifyAll();
                dataArchiveValidationService.VerifyAll();
                importService.VerifyAll();
                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                Assert.NotNull(result.Right.Id);
                Assert.Equal("Test data", result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFileName, result.Right.Path);
                Assert.NotNull(result.Right.MetaFileId);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var fileReferences = context.ReleaseFileReferences.ToList();

                Assert.Equal(3, fileReferences.Count);

                var dataFile = fileReferences
                    .Single(rfr => rfr.Filename == dataFileName);
                var metaFile = fileReferences
                    .Single(rfr => rfr.Filename == metaFileName);
                var zipFile = fileReferences
                    .Single(rfr => rfr.Filename == zipFileName);

                Assert.Equal(ReleaseFileTypes.Data, dataFile.ReleaseFileType);
                Assert.Equal(release.Id, dataFile.ReleaseId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(ReleaseFileTypes.Metadata, metaFile.ReleaseFileType);
                Assert.Equal(release.Id, metaFile.ReleaseId);

                Assert.Equal(ReleaseFileTypes.DataZip, zipFile.ReleaseFileType);
                Assert.Equal(release.Id, zipFile.ReleaseId);

                var releaseFiles = context.ReleaseFiles.ToList();

                Assert.Equal(2, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.ReleaseFileReferenceId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.ReleaseFileReferenceId == metaFile.Id));
            }
        }

        [Fact]
        public async void UploadDataFilesAsZip_Replacing()
        {
            var release = new Release();
            var originalDataFileReference = new ReleaseFileReference
            {
                Release = release,
                Filename = "original-data.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(new ReleaseFile
                {
                    Release = release,
                    ReleaseFileReference = originalDataFileReference
                });
                await context.SaveChangesAsync();
            }

            var dataFileName = "test-data.csv";
            var metaFileName = "test-data.meta.csv";
            var zipFileName = "test-data-archive.zip";

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var zipFormFile = CreateZipFormFileMock(zipFileName);
                var archiveFile = CreateDataArchiveFileMock(dataFileName, metaFileName);

                var subjectService = new Mock<ISubjectService>();

                subjectService
                    .Setup(s => s.GetAsync(originalDataFileReference.SubjectId.Value))
                    .ReturnsAsync(
                        new Subject
                        {
                            Name = "Test data"
                        }
                    );

                var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>();

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile.Object))
                    .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

                var dataArchiveValidationService = new Mock<IDataArchiveValidationService>();

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(release.Id, zipFormFile.Object))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile.Object));

                var importService = new Mock<IImportService>();

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

                var blobStorageService = new Mock<IBlobStorageService>();

                var zipBlobPath = AdminReleasePath(release.Id, ReleaseFileTypes.DataZip, zipFileName);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipBlobPath))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: zipBlobPath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                name: "Test data",
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseFilesService(
                    context,
                    blobStorageService.Object,
                    importService: importService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object,
                    subjectService: subjectService.Object
                );

                var result = await service.UploadDataFilesAsZip(
                    release.Id,
                    zipFormFile.Object,
                    userName: "test@test.com",
                    replacingFileId: originalDataFileReference.Id);

                subjectService.VerifyAll();
                fileUploadsValidatorService.VerifyAll();
                dataArchiveValidationService.VerifyAll();
                importService.VerifyAll();
                blobStorageService.VerifyAll();

                Assert.True(result.IsRight);

                Assert.NotNull(result.Right.Id);
                Assert.Equal("Test data", result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFileName, result.Right.Path);
                Assert.NotNull(result.Right.MetaFileId);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var fileReferences = context.ReleaseFileReferences.ToList();

                Assert.Equal(4, fileReferences.Count);

                var originalDataFile = fileReferences
                    .Single(rfr => rfr.Filename == originalDataFileReference.Filename);
                var dataFile = fileReferences
                    .Single(rfr => rfr.Filename == dataFileName);
                var metaFile = fileReferences
                    .Single(rfr => rfr.Filename == metaFileName);
                var zipFile = fileReferences
                    .Single(rfr => rfr.Filename == zipFileName);

                Assert.Equal(ReleaseFileTypes.Data, originalDataFile.ReleaseFileType);
                Assert.Equal(release.Id, originalDataFile.ReleaseId);
                Assert.Null(originalDataFile.SourceId);

                Assert.Equal(ReleaseFileTypes.Data, dataFile.ReleaseFileType);
                Assert.Equal(release.Id, dataFile.ReleaseId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(ReleaseFileTypes.Metadata, metaFile.ReleaseFileType);
                Assert.Equal(release.Id, metaFile.ReleaseId);

                Assert.Equal(ReleaseFileTypes.DataZip, zipFile.ReleaseFileType);
                Assert.Equal(release.Id, zipFile.ReleaseId);

                var releaseFiles = context.ReleaseFiles.ToList();

                Assert.Equal(3, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.ReleaseFileReferenceId == originalDataFileReference.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.ReleaseFileReferenceId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.ReleaseFileReferenceId == metaFile.Id));
            }
        }

        private static Mock<IFormFile> CreateZipFormFileMock(string fileName)
        {
            var zipFile = new Mock<IFormFile>();

            zipFile.SetupGet(f => f.FileName)
                .Returns(fileName);

            return zipFile;
        }


        private static Mock<IDataArchiveFile> CreateDataArchiveFileMock(
            string dataFileName,
            string metaFileName)
        {
            var dataArchiveFile = new Mock<IDataArchiveFile>();

            dataArchiveFile
                .SetupGet(f => f.DataFileName)
                .Returns(dataFileName);

            dataArchiveFile
                .SetupGet(f => f.MetaFileName)
                .Returns(metaFileName);

            return dataArchiveFile;
        }

        private static ReleaseFilesService SetupReleaseFilesService(
            ContentDbContext context,
            IBlobStorageService blobStorageService = null,
            IUserService userService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IImportService importService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            ISubjectService subjectService = null,
            IDataArchiveValidationService dataArchiveValidationService = null,
            IImportStatusService importStatusService = null)
        {
            return new ReleaseFilesService(
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                context,
                importService ?? new Mock<IImportService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object
            );
        }
    }
}