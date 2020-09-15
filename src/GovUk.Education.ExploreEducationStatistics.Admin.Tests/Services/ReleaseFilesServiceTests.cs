using System;
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
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            Filename = "test-data.meta.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata
                        }
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
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            Release = release,
                            SubjectId = subject.Id,
                            Filename = "test-data.meta.csv",
                            ReleaseFileType = ReleaseFileTypes.Metadata
                        }
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
                Assert.Equal(blobPath, fileInfo.Path);
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
                Assert.Equal("test-data-archive.zip", fileInfo.FileName);
                Assert.Equal("zip", fileInfo.Extension);
                Assert.Equal(zipBlobPath, fileInfo.Path);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal("1 Mb", fileInfo.Size);
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