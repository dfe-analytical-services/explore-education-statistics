#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseRepository;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServiceTest
    {
        private readonly User _user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Delete()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                Order = 1,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Source = zipFile
                }
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            // The deleting shouldn't effect the order of other ReleaseFiles
            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                Order = 0,
                File = new File { Type = FileType.Data },
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                Order = 2,
                File = new File { Type = FileType.Data },
            };
            var releaseFile4 = new ReleaseFile
            {
                Release = release,
                Order = 3,
                File = new File { Type = FileType.Data },
            };
            var releaseFile4Replacement = new ReleaseFile
            {
                Release = release,
                Order = 3,
                File = new File
                {
                    Type = FileType.Data,
                    Replacing = releaseFile4.File,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(zipFile);
                await contentDbContext.AddRangeAsync(releaseDataFile, releaseMetaFile);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    releaseFile1, releaseFile3, releaseFile4, releaseFile4Replacement);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(releaseDataFile.File.Id))
                .Returns(Task.CompletedTask);

            // test that the deletion of the main data and metadata files completed, as well as any zip files that
            // were uploaded
            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, It.IsIn(
                        releaseDataFile.Path(), releaseMetaFile.Path(), zipFile.Path())))
                .Returns(Task.CompletedTask);

            // test that the deletion of any remaining batch files went ahead for this particular data file
            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    releaseDataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(release.Id,
                    releaseDataFile.File.Id,
                    FileType.Data))
                .ReturnsAsync(releaseDataFile.File);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(release.Id, releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseMetaFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(releaseDataFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(releaseDataFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(releaseMetaFile.File.Id));

                Assert.Null(await contentDbContext.Files.FindAsync(zipFile.Id));

                var dbReleaseFile1 = contentDbContext.ReleaseFiles.Single(rf => rf.Id == releaseFile1.Id);
                Assert.Equal(0, dbReleaseFile1.Order);

                var dbReleaseFile3 = contentDbContext.ReleaseFiles.Single(rf => rf.Id == releaseFile3.Id);
                Assert.Equal(2, dbReleaseFile3.Order);

                var dbReleaseFile4 = contentDbContext.ReleaseFiles.Single(rf => rf.Id == releaseFile4.Id);
                Assert.Equal(3, dbReleaseFile4.Order);

                var dbReleaseFile4Replacement =
                    contentDbContext.ReleaseFiles.Single(rf => rf.Id == releaseFile4Replacement.Id);
                Assert.Equal(3, dbReleaseFile4Replacement.Order);
            }
        }

        [Fact]
        public async Task Delete_DeleteReplacementFiles()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var zipFile = new File
            {
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataFile = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                Filename = "data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var replacementZipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "replacement.zip",
                Type = DataZip,
                SubjectId = replacementSubject.Id
            };

            var replacementDataFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = dataFile,
                Source = replacementZipFile
            };

            dataFile.ReplacedBy = replacementDataFile;

            var replacementMetaFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "replacement.meta.csv",
                Type = Metadata,
                SubjectId = replacementSubject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                File = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                File = metaFile
            };

            var replacementReleaseDataFile = new ReleaseFile
            {
                Release = release,
                File = replacementDataFile
            };

            var replacementReleaseMetaFile = new ReleaseFile
            {
                Release = release,
                File = replacementMetaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(zipFile, dataFile, metaFile,
                    replacementZipFile, replacementDataFile, replacementMetaFile);
                await contentDbContext.AddRangeAsync(releaseDataFile, releaseMetaFile,
                    replacementReleaseDataFile, replacementReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(replacementDataFile.Id))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock => mock.DeleteBlob(PrivateReleaseFiles,
                    It.IsIn(replacementDataFile.Path(), replacementMetaFile.Path(), replacementZipFile.Path())))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    replacementDataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(release.Id,
                    replacementDataFile.Id,
                    FileType.Data))
                .ReturnsAsync(replacementDataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(release.Id, replacementDataFile.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, replacementDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                        mock.DeleteBlob(PrivateReleaseFiles, replacementMetaFile.Path()),
                    Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, replacementZipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(replacementDataFile.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(replacementReleaseDataFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(replacementDataFile.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(replacementReleaseMetaFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(replacementMetaFile.Id));

                Assert.Null(await contentDbContext.Files.FindAsync(replacementZipFile.Id));

                // Check that original file remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(metaFile.Id));

                Assert.NotNull(await contentDbContext.Files.FindAsync(zipFile.Id));

                // Check that the reference to the replacement is removed
                Assert.Null((await contentDbContext.Files.FindAsync(dataFile.Id))?.ReplacedById);
            }
        }

        [Fact]
        public async Task Delete_DeleteFilesFromAmendment()
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

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                File = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                File = metaFile
            };

            var amendmentReleaseDataFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = dataFile
            };

            var amendmentReleaseMetaFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = metaFile
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

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    dataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(amendmentRelease.Id,
                    dataFile.Id,
                    FileType.Data))
                .ReturnsAsync(dataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(amendmentRelease.Id, dataFile.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseDataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseMetaFile.Id));

                // Check that the non-amendment Release files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(metaFile.Id));

                Assert.NotNull(await contentDbContext.Files.FindAsync(zipFile.Id));
            }
        }

        [Fact]
        public async Task DeleteAll()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "chart.png",
                    Type = Chart
                }
            };

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Source = zipFile
                }
            };

            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(zipFile);
                await contentDbContext.AddRangeAsync(ancillaryReleaseFile, chartReleaseFile, dataReleaseFile,
                    metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(dataReleaseFile.File.Id))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock => mock.DeleteBlob(PrivateReleaseFiles,
                    It.IsIn(dataReleaseFile.Path(), metaReleaseFile.Path(), zipFile.Path())))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    dataReleaseFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(release.Id,
                    dataReleaseFile.File.Id,
                    FileType.Data))
                .ReturnsAsync(dataReleaseFile.File);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, metaReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(dataReleaseFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(dataReleaseFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(dataReleaseFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(metaReleaseFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(metaReleaseFile.File.Id));

                Assert.Null(await contentDbContext.Files.FindAsync(zipFile.Id));

                // Check that other file types remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));
                Assert.NotNull(
                    await contentDbContext.Files.FindAsync(ancillaryReleaseFile.File
                        .Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartReleaseFile.Id));
                Assert.NotNull(
                    await contentDbContext.Files.FindAsync(chartReleaseFile.File.Id));
            }
        }

        [Fact]
        public async Task DeleteAll_FileFromAmendment()
        {
            var release = new Release();

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id,
            };

            var dataFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                File = dataFile
            };

            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = metaFile
            };

            var amendmentReleaseDataFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = dataFile
            };

            var amendmentReleaseMetaFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = metaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.AddRangeAsync(dataReleaseFile, metaReleaseFile, amendmentReleaseDataFile,
                    amendmentReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            blobStorageService.Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    dataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(amendmentRelease.Id,
                    dataFile.Id,
                    FileType.Data))
                .ReturnsAsync(dataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the data and meta files are unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseDataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseMetaFile.Id));

                // Check that the data, meta and zip files linked to the previous version remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataReleaseFile.Id));
                Assert.NotNull(
                    await contentDbContext.Files.FindAsync(dataReleaseFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(metaReleaseFile.Id));
                Assert.NotNull(
                    await contentDbContext.Files.FindAsync(metaReleaseFile.File.Id));

                Assert.NotNull(await contentDbContext.Files.FindAsync(zipFile.Id));
            }
        }

        [Fact]
        public async Task DeleteAll_ReleaseNotFound()
        {
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                blobStorageService: blobStorageService.Object,
                dataImportService: dataImportService.Object);

            var result = await service.DeleteAll(Guid.NewGuid());

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
        }

        [Fact]
        public async Task DeleteAll_NoFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
            }
        }

        [Fact]
        public async Task GetInfo()
        {
            var release = new Release();

            var dataReleaseFile = new ReleaseFile
            {
                Name = "Test data",
                Release = release,
                File = new File
                {
                    Filename = "test-data.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };

            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "test-data.meta.csv",
                    Type = Metadata
                }
            };

            var dataImport = new DataImport
            {
                File = dataReleaseFile.File,
                MetaFile = metaReleaseFile.File,
                TotalRows = 200,
                Status = COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(dataReleaseFile, metaReleaseFile);
                await contentDbContext.DataImports.AddAsync(dataImport);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.GetInfo(
                    release.Id,
                    dataReleaseFile.File.Id
                );

                var fileInfo = result.AssertRight();

                Assert.Equal(dataReleaseFile.File.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaReleaseFile.File.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("10 Kb", fileInfo.Size);
                Assert.Equal(dataReleaseFile.File.Created, fileInfo.Created);
                Assert.Equal(COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetInfo_ReleaseNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // A file for another release exists
                var anotherRelease = new Release();
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = anotherRelease,
                        File = new File
                        {
                            Filename = "test-data.csv",
                            Type = Metadata
                        }
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext);

                var result = await service.GetInfo(
                    Guid.NewGuid(),
                    Guid.NewGuid()
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetInfo_FileNotForRelease()
        {
            var release = new Release();
            var otherRelease = new Release();

            var dataFile = new File
            {
                Filename = "test-data.csv",
                Type = FileType.Data,
                CreatedById = _user.Id
            };
            var metaFile = new File
            {
                Filename = "test-data.meta.csv",
                Type = Metadata
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = dataFile
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = metaFile
                    }
                );
                await contentDbContext.AddAsync(otherRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext);

                var result = await service.GetInfo(
                    otherRelease.Id,
                    dataFile.Id
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetInfo_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

            var dataFile = new File
            {
                Filename = "test-data.csv",
                ContentLength = 10240,
                Type = FileType.Data,
                Created = DateTime.UtcNow,
                CreatedById = _user.Id
            };

            var metaFile = new File
            {
                Filename = "test-data.meta.csv",
                Type = Metadata,
            };

            var dataOriginalReleaseFile = new ReleaseFile
            {
                Release = originalRelease,
                Name = "Test data",
                File = dataFile,
            };

            var metaOriginalReleaseFile = new ReleaseFile
            {
                Release = originalRelease,
                File = metaFile,
            };

            var dataAmendedReleaseFile = new ReleaseFile
            {
                Release = amendedRelease,
                Name = "Test data amended",
                File = dataFile,
            };

            var metaAmendedReleaseFile = new ReleaseFile
            {
                Release = amendedRelease,
                File = metaFile,
            };

            var dataImport = new DataImport
            {
                File = dataFile,
                MetaFile = metaFile,
                TotalRows = 200,
                Status = COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(originalRelease, amendedRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    dataOriginalReleaseFile,
                    dataAmendedReleaseFile,
                    metaOriginalReleaseFile,
                    metaAmendedReleaseFile);
                await contentDbContext.DataImports.AddAsync(dataImport);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.GetInfo(
                    amendedRelease.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data amended", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("10 Kb", fileInfo.Size);
                Assert.Equal(dataFile.Created, fileInfo.Created);
                Assert.Equal(COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task ReorderDataFiles()
        {
            var release = new Release();
            var releaseDataFile1 = new ReleaseFile
            {
                Release = release,
                Order = 5,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile2 = new ReleaseFile
            {
                Release = release,
                Order = 3,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile3 = new ReleaseFile
            {
                Release = release,
                Order = 1,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile4 = new ReleaseFile
            {
                Release = release,
                Order = 2,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile5 = new ReleaseFile
            {
                Release = release,
                Order = 0,
                File = new File
                {
                    Type = FileType.Data,
                },
            };

            var releaseMetaFile1 = new ReleaseFile
            {
                Release = releaseDataFile1.Release,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile2 = new ReleaseFile
            {
                Release = releaseDataFile2.Release,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile3 = new ReleaseFile
            {
                Release = releaseDataFile3.Release,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile4 = new ReleaseFile
            {
                Release = releaseDataFile4.Release,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile5 = new ReleaseFile
            {
                Release = releaseDataFile5.Release,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };

            var dataImports = new List<DataImport>
            {
                new()
                {
                    File = releaseDataFile1.File,
                    MetaFile = releaseMetaFile1.File,
                },
                new()
                {
                    File = releaseDataFile2.File,
                    MetaFile = releaseMetaFile2.File,
                },
                new()
                {
                    File = releaseDataFile3.File,
                    MetaFile = releaseMetaFile3.File,
                },
                new()
                {
                    File = releaseDataFile4.File,
                    MetaFile = releaseMetaFile4.File,
                },
                new()
                {
                    File = releaseDataFile5.File,
                    MetaFile = releaseMetaFile5.File,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    releaseDataFile1, releaseDataFile2, releaseDataFile3, releaseDataFile4, releaseDataFile5,
                    releaseMetaFile1, releaseMetaFile2, releaseMetaFile3, releaseMetaFile4, releaseMetaFile5
                );
                await contentDbContext.DataImports.AddRangeAsync(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext
                );

                var result = await service.ReorderDataFiles(
                    release.Id,
                    new List<Guid>
                    {
                        releaseDataFile1.File.Id,
                        releaseDataFile2.File.Id,
                        releaseDataFile3.File.Id,
                        releaseDataFile4.File.Id,
                        releaseDataFile5.File.Id,
                    });

                var dataFiles = result.AssertRight().ToList();
                Assert.Equal(5, dataFiles.Count);

                Assert.Equal(releaseDataFile1.File.Id, dataFiles[0].Id);
                Assert.Equal(releaseDataFile2.File.Id, dataFiles[1].Id);
                Assert.Equal(releaseDataFile3.File.Id, dataFiles[2].Id);
                Assert.Equal(releaseDataFile4.File.Id, dataFiles[3].Id);
                Assert.Equal(releaseDataFile5.File.Id, dataFiles[4].Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var dbDataFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.ReleaseId == release.Id && rf.File.Type == FileType.Data)
                    .ToList();

                var dbDataFile1 = dbDataFiles.Find(rf => rf.Id == releaseDataFile1.Id);
                Assert.NotNull(dbDataFile1);
                Assert.Equal(0, dbDataFile1!.Order);

                var dbDataFile2 = dbDataFiles.Find(rf => rf.Id == releaseDataFile2.Id);
                Assert.NotNull(dbDataFile2);
                Assert.Equal(1, dbDataFile2!.Order);

                var dbDataFile3 = dbDataFiles.Find(rf => rf.Id == releaseDataFile3.Id);
                Assert.NotNull(dbDataFile3);
                Assert.Equal(2, dbDataFile3!.Order);

                var dbDataFile4 = dbDataFiles.Find(rf => rf.Id == releaseDataFile4.Id);
                Assert.NotNull(dbDataFile4);
                Assert.Equal(3, dbDataFile4!.Order);

                var dbDataFile5 = dbDataFiles.Find(rf => rf.Id == releaseDataFile5.Id);
                Assert.NotNull(dbDataFile5);
                Assert.Equal(4, dbDataFile5!.Order);

                var dbMetaFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.ReleaseId == release.Id && rf.File.Type == FileType.Metadata)
                    .ToList();

                // Non-FileType.Data files should default to Order 0
                var dbMetaFile1 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile1.Id);
                Assert.NotNull(dbMetaFile1);
                Assert.Equal(0, dbMetaFile1!.Order);

                var dbMetaFile2 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile2.Id);
                Assert.NotNull(dbMetaFile2);
                Assert.Equal(0, dbMetaFile2!.Order);

                var dbMetaFile3 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile3.Id);
                Assert.NotNull(dbMetaFile3);
                Assert.Equal(0, dbMetaFile3!.Order);

                var dbMetaFile4 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile4.Id);
                Assert.NotNull(dbMetaFile4);
                Assert.Equal(0, dbMetaFile4!.Order);

                var dbMetaFile5 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile5.Id);
                Assert.NotNull(dbMetaFile5);
                Assert.Equal(0, dbMetaFile5!.Order);
            }
        }

        [Fact]
        public async Task ListAll()
        {
            var release = new Release();
            var dataReleaseFile1 = new ReleaseFile
            {
                Release = release,
                Name = "Test subject 1",
                File = new File
                {
                    Filename = "test-data-1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                }
            };
            var dataReleaseFile2 = new ReleaseFile
            {
                Release = release,
                Name = "Test subject 2",
                File = new File
                {
                    Filename = "Test data 2.csv",
                    ContentLength = 20480,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "Test data 2.meta.csv",
                    Type = Metadata,
                }
            };

            var dataImports = new List<DataImport>
            {
                new()
                {
                    File = dataReleaseFile1.File,
                    MetaFile = metaReleaseFile1.File,
                    TotalRows = 200,
                    Status = COMPLETE
                },
                new()
                {
                    File = dataReleaseFile2.File,
                    MetaFile = metaReleaseFile2.File,
                    TotalRows = 400,
                    Status = STAGE_2
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataReleaseFile2,
                    metaReleaseFile2);
                await contentDbContext.DataImports.AddRangeAsync(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Equal(2, files.Count);

                Assert.Equal(dataReleaseFile1.File.Id, files[0].Id);
                Assert.Equal("Test subject 1", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(metaReleaseFile1.File.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("10 Kb", files[0].Size);
                Assert.Equal(dataReleaseFile1.File.Created, files[0].Created);
                Assert.Equal(COMPLETE, files[0].Status);

                Assert.Equal(dataReleaseFile2.File.Id, files[1].Id);
                Assert.Equal("Test subject 2", files[1].Name);
                Assert.Equal("Test data 2.csv", files[1].FileName);
                Assert.Equal("csv", files[1].Extension);
                Assert.Equal(metaReleaseFile2.File.Id, files[1].MetaFileId);
                Assert.Equal("Test data 2.meta.csv", files[1].MetaFileName);
                Assert.Equal(_user.Email, files[1].UserName);
                Assert.Equal(400, files[1].Rows);
                Assert.Equal("20 Kb", files[1].Size);
                Assert.Equal(dataReleaseFile2.File.Created, files[1].Created);
                Assert.Equal(STAGE_2, files[1].Status);
            }
        }

        [Fact]
        public async Task ListAll_FiltersCorrectly()
        {
            var release1 = new Release();
            var release2 = new Release();

            var dataRelease1File = new ReleaseFile
            {
                Release = release1,
                Name = "Test data",
                File = new File
                {
                    Filename = "test-data-1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaRelease1File = new ReleaseFile
            {
                Release = release1,
                File = new File
                {
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                }
            };

            var dataRelease2File = new ReleaseFile
            {
                Release = release2,
                Name = "Test data 2",
                File = new File
                {
                    Filename = "test-data-2.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    CreatedById = _user.Id
                }
            };
            var metaRelease2File = new ReleaseFile
            {
                Release = release2,
                File = new File
                {
                    Filename = "test-data-2.meta.csv",
                    Type = Metadata
                }
            };

            var ancillaryRelease1File = new ReleaseFile
            {
                Release = release1,
                File = new File
                {
                    Filename = "ancillary-file.pdf",
                    ContentLength = 10240,
                    Type = Ancillary
                }
            };

            var dataImports = new List<DataImport>
            {
                new()
                {
                    File = dataRelease1File.File,
                    MetaFile = metaRelease1File.File,
                    TotalRows = 200,
                    Status = COMPLETE
                },
                new()
                {
                    File = dataRelease2File.File,
                    MetaFile = metaRelease2File.File,
                    TotalRows = 400,
                    Status = STAGE_2
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release1, release2);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    dataRelease1File,
                    metaRelease1File,
                    dataRelease2File,
                    metaRelease2File,
                    ancillaryRelease1File); // Not FileType.Data
                await contentDbContext.AddRangeAsync(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(release1.Id);

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataRelease1File.File.Id, files[0].Id);
                Assert.Equal("Test data", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(metaRelease1File.File.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("10 Kb", files[0].Size);
                Assert.Equal(dataRelease1File.File.Created, files[0].Created);
                Assert.Equal(COMPLETE, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

            var dataReleaseFile1 = new ReleaseFile
            {
                Release = originalRelease,
                Name = "Test subject 1",
                File = new File
                {
                    Filename = "test-data-1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow.AddDays(-1),
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile1 = new ReleaseFile
            {
                Release = originalRelease,
                File = new File
                {
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                }
            };

            var dataFile2 = new File
            {
                Filename = "test-data-2.csv",
                ContentLength = 20480,
                Type = FileType.Data,
                Created = DateTime.UtcNow,
                CreatedById = _user.Id
            };
            var metaFile2 = new File
            {
                Filename = "test-data-2.meta.csv",
                Type = Metadata
            };
            var dataOriginalReleaseFile2 = new ReleaseFile
            {
                Release = originalRelease,
                Name = "Test subject 2",
                File = dataFile2,
            };
            var metaOriginalReleaseFile2 = new ReleaseFile
            {
                Release = originalRelease,
                File = metaFile2,
            };
            var dataAmendedReleaseFile2 = new ReleaseFile
            {
                Release = amendedRelease,
                Name = "Test subject 2 name change",
                File = dataFile2,
            };
            var metaAmendedReleaseFile2 = new ReleaseFile
            {
                Release = amendedRelease,
                File = metaFile2,
            };

            var dataImports = new List<DataImport>
            {
                new()
                {
                    File = dataReleaseFile1.File,
                    MetaFile = metaReleaseFile1.File,
                    TotalRows = 400,
                    Status = STAGE_2
                },
                new()
                {
                    File = dataFile2,
                    MetaFile = metaFile2,
                    TotalRows = 200,
                    Status = COMPLETE
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(originalRelease, amendedRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataOriginalReleaseFile2,
                    metaOriginalReleaseFile2,
                    // Only second data file is attached to amended release
                    dataAmendedReleaseFile2,
                    metaAmendedReleaseFile2);
                await contentDbContext.DataImports.AddRangeAsync(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(amendedRelease.Id);

                Assert.True(result.IsRight);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile2.Id, files[0].Id);
                Assert.Equal("Test subject 2 name change", files[0].Name);
                Assert.Equal("test-data-2.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(metaFile2.Id, files[0].MetaFileId);
                Assert.Equal("test-data-2.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("20 Kb", files[0].Size);
                Assert.Equal(dataFile2.Created, files[0].Created);
                Assert.Equal(COMPLETE, files[0].Status);
            }
        }

        [Fact]
        public async Task Upload()
        {
            const string subjectName = "Test Subject";
            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";

            var release = new Release
            {
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataFormFile = CreateFormFileMock(dataFileName).Object;
            var metaFormFile = CreateFormFileMock(metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateSubjectName(release.Id, subjectName))
                    .ReturnsAsync(Unit.Instance);

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataFilesForUpload(
                        release.Id,
                        dataFormFile,
                        metaFormFile,
                        null))
                    .ReturnsAsync(Unit.Instance);

                dataImportService
                    .Setup(s => s.Import(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName)))
                    .ReturnsAsync(new DataImport
                    {
                        Status = QUEUED
                    });

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        dataFormFile
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        metaFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.Upload(
                    releaseId: release.Id,
                    dataFormFile: dataFormFile,
                    metaFormFile: metaFormFile,
                    subjectName: subjectName);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, dataImportService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(subjectName, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal(_user.Email, result.Right.UserName);
                Assert.Null(result.Right.Rows);
                Assert.Equal("10 Kb", result.Right.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Right.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Right.Status);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();

                Assert.Equal(2, files.Count);

                var dataFile = files
                    .Single(f => f.Filename == dataFileName);
                var metaFile = files
                    .Single(f => f.Filename == metaFileName);

                Assert.Equal(10240, dataFile.ContentLength);
                Assert.Equal("text/csv", dataFile.ContentType);
                Assert.Equal(FileType.Data, dataFile.Type);

                Assert.Equal(10240, metaFile.ContentLength);
                Assert.Equal("text/csv", metaFile.ContentType);
                Assert.Equal(Metadata, metaFile.Type);

                var releaseFiles = contentDbContext.ReleaseFiles.ToList();

                Assert.Equal(2, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == metaFile.Id));
            }
        }

        [Fact]
        public async Task Upload_Replacing()
        {
            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";

            var release = new Release
            {
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                }
            };

            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var originalDataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test data",
                Order = 93,
                File = new File
                {
                    Filename = "original-data.csv",
                    Type = FileType.Data,
                    SubjectId = originalSubject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(originalDataReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var originalReleaseSubject = new ReleaseSubject
            {
                ReleaseId = release.Id,
                SubjectId = originalSubject.Id
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(originalSubject);
                await statisticsDbContext.AddAsync(originalReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataFormFile = CreateFormFileMock(dataFileName).Object;
            var metaFormFile = CreateFormFileMock(metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataFilesForUpload(
                        release.Id,
                        dataFormFile,
                        metaFormFile,
                        It.Is<File>(f => f.Id == originalDataReleaseFile.File.Id)))
                    .ReturnsAsync(Unit.Instance);

                dataImportService
                    .Setup(s => s.Import(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName)))
                    .ReturnsAsync(new DataImport
                    {
                        Status = QUEUED
                    });

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        dataFormFile
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        metaFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.Upload(
                    releaseId: release.Id,
                    dataFormFile: dataFormFile,
                    metaFormFile: metaFormFile,
                    replacingFileId: originalDataReleaseFile.File.Id);

                var dataFileInfo = result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, dataImportService);

                Assert.True(dataFileInfo.Id.HasValue);
                Assert.Equal(originalDataReleaseFile.Name, dataFileInfo.Name);
                Assert.Equal(dataFileName, dataFileInfo.FileName);
                Assert.Equal("csv", dataFileInfo.Extension);
                Assert.True(dataFileInfo.MetaFileId.HasValue);
                Assert.Equal(metaFileName, dataFileInfo.MetaFileName);
                Assert.Equal(_user.Email, dataFileInfo.UserName);
                Assert.Null(dataFileInfo.Rows);
                Assert.Equal("10 Kb", dataFileInfo.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(dataFileInfo.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, dataFileInfo.Status);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Equal(2, subjects.Count);
                Assert.NotNull(subjects.Find(s => s.Id == originalSubject.Id));

                var replacementSubject = subjects.Find(s => s.Id != originalSubject.Id);
                Assert.NotNull(replacementSubject);

                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Equal(2, releaseSubjects.Count);
                Assert.Contains(releaseSubjects,
                    rs =>
                        rs.SubjectId == originalSubject.Id
                        && rs.ReleaseId == release.Id);
                Assert.Contains(releaseSubjects,
                    rs =>
                        rs.SubjectId == replacementSubject!.Id
                        && rs.ReleaseId == release.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();

                Assert.Equal(3, files.Count);

                var originalDataFile = files
                    .Single(f => f.Filename == originalDataReleaseFile.File.Filename);
                var dataFile = files
                    .Single(f => f.Filename == dataFileName);
                var metaFile = files
                    .Single(f => f.Filename == metaFileName);

                Assert.Equal(FileType.Data, originalDataFile.Type);
                Assert.Equal(dataFile.Id, originalDataFile.ReplacedById);

                Assert.Equal(10240, dataFile.ContentLength);
                Assert.Equal("text/csv", dataFile.ContentType);
                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);

                Assert.Equal(10240, metaFile.ContentLength);
                Assert.Equal("text/csv", metaFile.ContentType);
                Assert.Equal(Metadata, metaFile.Type);

                var releaseFiles = contentDbContext.ReleaseFiles.ToList();

                Assert.Equal(3, releaseFiles.Count);

                var dbOriginalReleaseDataFile = Assert.Single(releaseFiles
                    .Where(rf =>
                        rf.ReleaseId == release.Id
                        && rf.FileId == originalDataFile.Id)
                    .ToList());
                Assert.Equal(93, dbOriginalReleaseDataFile.Order);

                var dbReleaseDataFile = Assert.Single(releaseFiles
                    .Where(rf =>
                        rf.ReleaseId == release.Id
                        && rf.FileId == dataFile.Id)
                    .ToList());
                Assert.Equal(93, dbReleaseDataFile.Order);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == metaFile.Id));
            }
        }

        [Fact]
        public async Task Upload_Order()
        {
            const string subjectName = "Test Subject";
            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";

            var release = new Release
            {
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                },
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 0,
                },
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 1,
                },
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 3,
                },
                new () // Ancillary files should be ignored
                {
                    Release = release,
                    File = new File { Type = FileType.Ancillary },
                    Order = 5,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var dataFormFile = CreateFormFileMock(dataFileName).Object;
            var metaFormFile = CreateFormFileMock(metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateSubjectName(release.Id, subjectName))
                    .ReturnsAsync(Unit.Instance);

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataFilesForUpload(
                        release.Id,
                        dataFormFile,
                        metaFormFile,
                        null))
                    .ReturnsAsync(Unit.Instance);

                dataImportService
                    .Setup(s => s.Import(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName)))
                    .ReturnsAsync(new DataImport());

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        dataFormFile
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        metaFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.Upload(
                    releaseId: release.Id,
                    dataFormFile: dataFormFile,
                    metaFormFile: metaFormFile,
                    subjectName: subjectName);

                var dataFileInfo = result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, dataImportService);

                Assert.True(dataFileInfo.Id.HasValue);
                Assert.Equal(dataFileName, dataFileInfo.FileName);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();
                Assert.Equal(6, files.Count);

                var dbDataReleaseFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.File.Type == FileType.Data)
                    .ToList();
                Assert.Equal(4, dbDataReleaseFiles.Count);

                Assert.Equal(0, dbDataReleaseFiles[0].Order);
                Assert.Equal(1, dbDataReleaseFiles[1].Order);
                Assert.Equal(3, dbDataReleaseFiles[2].Order);
                Assert.Equal(4, dbDataReleaseFiles[3].Order); // Highest Order of existing releaseFiles is 3 then + 1
                Assert.Equal(subjectName, dbDataReleaseFiles[3].Name);


                var dbMetaReleaseFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.File.Type == FileType.Metadata)
                    .ToList();
                var dbMetaReleaseFile = Assert.Single(dbMetaReleaseFiles);

                Assert.Equal(metaFileName, dbMetaReleaseFile.File.Filename);
                Assert.Equal(0, dbMetaReleaseFile.Order); // Files that aren't FileType.Data have Order set to 0
            }
        }

        [Fact]
        public async Task UploadAsZip()
        {
            const string subjectName = "Test Subject";

            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";
            const string zipFileName = "test-data-archive.zip";

            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);

                await contentDbContext.SaveChangesAsync();
            }

            var zipFormFile = CreateFormFileMock(zipFileName, "application/zip").Object;
            var archiveFile = CreateDataArchiveFileMock(dataFileName, metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataArchiveValidationService = new Mock<IDataArchiveValidationService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateSubjectName(release.Id, subjectName))
                    .ReturnsAsync(Unit.Instance);

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile, null))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                dataImportService
                    .Setup(s => s.ImportZip(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        It.Is<File>(file => file.Type == DataZip && file.Filename == zipFileName)))
                    .ReturnsAsync(new DataImport
                    {
                        Status = QUEUED
                    });

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip))),
                        zipFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = (await service.UploadAsZip(
                    releaseId: release.Id,
                    zipFormFile: zipFormFile,
                    subjectName: subjectName)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataArchiveValidationService,
                    fileUploadsValidatorService,
                    dataImportService);

                Assert.True(result.Id.HasValue);
                Assert.Equal(subjectName, result.Name);
                Assert.Equal(dataFileName, result.FileName);
                Assert.Equal("csv", result.Extension);
                Assert.True(result.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.MetaFileName);
                Assert.Equal(_user.Email, result.UserName);
                Assert.Null(result.Rows);
                Assert.Equal("1 Mb", result.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Status);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();

                Assert.Equal(3, files.Count);

                var dataFile = files
                    .Single(f => f.Filename == dataFileName);
                var metaFile = files
                    .Single(f => f.Filename == metaFileName);
                var zipFile = files
                    .Single(f => f.Filename == zipFileName);

                Assert.Equal(1048576, dataFile.ContentLength);
                Assert.Equal("text/csv", dataFile.ContentType);
                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(1024, metaFile.ContentLength);
                Assert.Equal("text/csv", metaFile.ContentType);
                Assert.Equal(Metadata, metaFile.Type);

                Assert.Equal(10240, zipFile.ContentLength);
                Assert.Equal("application/zip", zipFile.ContentType);
                Assert.Equal(DataZip, zipFile.Type);

                var releaseFiles = contentDbContext.ReleaseFiles.ToList();

                Assert.Equal(2, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == metaFile.Id));
            }
        }

        [Fact]
        public async Task UploadAsZip_Order()
        {
            const string subjectName = "Test Subject";

            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";
            const string zipFileName = "test-data-archive.zip";

            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 0,
                },
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 1,
                },
                new ()
                {
                    Release = release,
                    File = new File { Type = FileType.Data },
                    Order = 3,
                },
                new () // Ancillary files should be ignored
                {
                    Release = release,
                    File = new File { Type = FileType.Ancillary },
                    Order = 5,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);

                await contentDbContext.SaveChangesAsync();
            }

            var zipFormFile = CreateFormFileMock(zipFileName).Object;
            var archiveFile = CreateDataArchiveFileMock(dataFileName, metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataArchiveValidationService = new Mock<IDataArchiveValidationService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateSubjectName(release.Id, subjectName))
                    .ReturnsAsync(Unit.Instance);

                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile, null))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                dataImportService
                    .Setup(s => s.ImportZip(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        It.Is<File>(file => file.Type == DataZip && file.Filename == zipFileName)))
                    .ReturnsAsync(new DataImport());

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip))),
                        zipFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.UploadAsZip(
                    releaseId: release.Id,
                    zipFormFile: zipFormFile,
                    subjectName: subjectName);

                var dataFileInfo = result.AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataArchiveValidationService,
                    fileUploadsValidatorService,
                    dataImportService);

                Assert.True(dataFileInfo.Id.HasValue);
                Assert.Equal(dataFileName, dataFileInfo.FileName);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();
                Assert.Equal(7, files.Count);

                Assert.Single(files.Where(f => f.Type == FileType.DataZip).ToList());


                var dbDataReleaseFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.File.Type == FileType.Data)
                    .ToList();
                Assert.Equal(4, dbDataReleaseFiles.Count);

                Assert.Equal(0, dbDataReleaseFiles[0].Order);
                Assert.Equal(1, dbDataReleaseFiles[1].Order);
                Assert.Equal(3, dbDataReleaseFiles[2].Order);
                Assert.Equal(4, dbDataReleaseFiles[3].Order); // Highest Order of existing releaseFiles is 3 then + 1
                Assert.Equal(subjectName, dbDataReleaseFiles[3].Name);


                var dbMetaReleaseFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.File.Type == FileType.Metadata)
                    .ToList();
                var dbMetaReleaseFile = Assert.Single(dbMetaReleaseFiles);

                Assert.Equal(metaFileName, dbMetaReleaseFile.File.Filename);
                Assert.Equal(0, dbMetaReleaseFile.Order); // Files that aren't FileType.Data have Order set to 0
            }
        }

        [Fact]
        public async Task UploadAsZip_Replacing()
        {
            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";
            const string zipFileName = "test-data-archive.zip";

            var release = new Release
            {
                ReleaseName = "2000",
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test theme"
                        }
                    }
                }
            };

            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var originalDataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Subject name",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "original-data.csv",
                    Type = FileType.Data,
                    SubjectId = originalSubject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(originalDataReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var originalReleaseSubject = new ReleaseSubject
            {
                ReleaseId = release.Id,
                SubjectId = originalSubject.Id
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(originalSubject);
                await statisticsDbContext.AddAsync(originalReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var zipFormFile = CreateFormFileMock(zipFileName, "application/zip").Object;
            var archiveFile = CreateDataArchiveFileMock(dataFileName, metaFileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var dataArchiveValidationService = new Mock<IDataArchiveValidationService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id,
                        archiveFile,
                        It.Is<File>(file => file.Id == originalDataReleaseFile.File.Id)))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                dataImportService
                    .Setup(s => s.ImportZip(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        It.Is<File>(file => file.Type == DataZip && file.Filename == zipFileName)))
                    .ReturnsAsync(new DataImport
                    {
                        Status = QUEUED
                    });

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip))),
                        zipFormFile
                    )).Returns(Task.CompletedTask);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = (await service.UploadAsZip(
                    release.Id,
                    zipFormFile,
                    replacingFileId: originalDataReleaseFile.File.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataArchiveValidationService,
                    fileUploadsValidatorService,
                    dataImportService);

                Assert.True(result.Id.HasValue);
                Assert.Equal(originalDataReleaseFile.Name, result.Name);
                Assert.Equal(dataFileName, result.FileName);
                Assert.Equal("csv", result.Extension);
                Assert.True(result.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.MetaFileName);
                Assert.Equal(_user.Email, result.UserName);
                Assert.Null(result.Rows);
                Assert.Equal("1 Mb", result.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Status);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Equal(2, subjects.Count);
                Assert.NotNull(subjects.Find(s => s.Id == originalSubject.Id));

                var replacementSubject = subjects.Find(s => s.Id != originalSubject.Id);
                Assert.NotNull(replacementSubject);

                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Equal(2, releaseSubjects.Count);
                Assert.Contains(releaseSubjects,
                    rs =>
                        rs.SubjectId == originalSubject.Id
                        && rs.ReleaseId == release.Id);
                Assert.Contains(releaseSubjects,
                    rs =>
                        rs.SubjectId == replacementSubject!.Id
                        && rs.ReleaseId == release.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var files = contentDbContext.Files.ToList();

                Assert.Equal(4, files.Count);

                var originalDataFile = files
                    .Single(f => f.Filename == originalDataReleaseFile.File.Filename);
                var dataFile = files
                    .Single(f => f.Filename == dataFileName);
                var metaFile = files
                    .Single(f => f.Filename == metaFileName);
                var zipFile = files
                    .Single(f => f.Filename == zipFileName);

                Assert.Equal(FileType.Data, originalDataFile.Type);
                Assert.Equal(dataFile.Id, originalDataFile.ReplacedById);
                Assert.Null(originalDataFile.SourceId);

                Assert.Equal(1048576, dataFile.ContentLength);
                Assert.Equal("text/csv", dataFile.ContentType);
                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(1024, metaFile.ContentLength);
                Assert.Equal("text/csv", metaFile.ContentType);
                Assert.Equal(Metadata, metaFile.Type);

                Assert.Equal(10240, zipFile.ContentLength);
                Assert.Equal("application/zip", zipFile.ContentType);
                Assert.Equal(DataZip, zipFile.Type);

                var releaseFiles = contentDbContext.ReleaseFiles.ToList();

                Assert.Equal(3, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == originalDataReleaseFile.File.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == metaFile.Id));
            }
        }

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext contentDbContext,
            StatisticsDbContext? statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IDataArchiveValidationService? dataArchiveValidationService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IFileRepository? fileRepository = null,
            IReleaseRepository? releaseRepository = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null,
            IDataImportService? dataImportService = null,
            IUserService? userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseDataFileService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                dataArchiveValidationService ?? Mock.Of<IDataArchiveValidationService>(Strict),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(Strict),
                fileRepository ?? new FileRepository(contentDbContext),
                releaseRepository ?? new ReleaseRepository(
                    contentDbContext,
                    statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>()),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
