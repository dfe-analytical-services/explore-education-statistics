#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServiceTests
    {
        private readonly DataFixture _fixture = new();

        private readonly User _user = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Delete()
        {
            var releaseVersion = new ReleaseVersion();

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
                ReleaseVersion = releaseVersion,
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
                ReleaseVersion = releaseVersion,
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
                ReleaseVersion = releaseVersion,
                Order = 0,
                File = new File { Type = FileType.Data },
            };
            var releaseFile3 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 2,
                File = new File { Type = FileType.Data },
            };
            var releaseFile4 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 3,
                File = new File { Type = FileType.Data },
            };
            var releaseFile4Replacement = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Files.Add(zipFile);
                contentDbContext.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile);
                contentDbContext.ReleaseFiles.AddRange(
                    releaseFile1, releaseFile3, releaseFile4, releaseFile4Replacement);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(releaseDataFile.File.Id))
                .Returns(Task.CompletedTask);

            // test that the deletion of the main data and metadata files completed, as well as any zip files that
            // were uploaded
            privateBlobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, It.IsIn(
                        releaseDataFile.Path(), releaseMetaFile.Path(), zipFile.Path())))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(releaseVersion.Id,
                    releaseDataFile.File.Id,
                    FileType.Data))
                .ReturnsAsync(releaseDataFile.File);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(releaseVersionId: releaseVersion.Id,
                    fileId: releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseDataFile.Path()), Times.Once());
                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseMetaFile.Path()), Times.Once());
                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(releaseDataFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(privateBlobStorageService,
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
            var releaseVersion = new ReleaseVersion();

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
                ReleaseVersion = releaseVersion,
                File = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = metaFile
            };

            var replacementReleaseDataFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementDataFile
            };

            var replacementReleaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementMetaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Files.AddRange(zipFile, dataFile, metaFile,
                    replacementZipFile, replacementDataFile, replacementMetaFile);
                contentDbContext.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile,
                    replacementReleaseDataFile, replacementReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(replacementDataFile.Id))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.DeleteBlob(PrivateReleaseFiles,
                    It.IsIn(replacementDataFile.Path(), replacementMetaFile.Path(), replacementZipFile.Path())))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(releaseVersion.Id,
                    replacementDataFile.Id,
                    FileType.Data))
                .ReturnsAsync(replacementDataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(releaseVersionId: releaseVersion.Id,
                    fileId: replacementDataFile.Id);

                Assert.True(result.IsRight);

                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, replacementDataFile.Path()), Times.Once());
                privateBlobStorageService.Verify(mock =>
                        mock.DeleteBlob(PrivateReleaseFiles, replacementMetaFile.Path()),
                    Times.Once());
                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, replacementZipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(replacementDataFile.Id), Times.Once());

                MockUtils.VerifyAllMocks(privateBlobStorageService,
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
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid()
            };

            var amendmentRelease = new ReleaseVersion
            {
                PreviousVersionId = releaseVersion.Id
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
                ReleaseVersion = releaseVersion,
                File = dataFile
            };

            var releaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = metaFile
            };

            var amendmentReleaseDataFile = new ReleaseFile
            {
                ReleaseVersion = amendmentRelease,
                File = dataFile
            };

            var amendmentReleaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = amendmentRelease,
                File = metaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendmentRelease);
                contentDbContext.Files.AddRange(zipFile, dataFile, metaFile);
                contentDbContext.ReleaseFiles.AddRange(
                    releaseDataFile, releaseMetaFile, amendmentReleaseDataFile, amendmentReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService.Setup(mock => mock.CheckFileExists(amendmentRelease.Id,
                    dataFile.Id,
                    FileType.Data))
                .ReturnsAsync(dataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(amendmentRelease.Id, dataFile.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(privateBlobStorageService,
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
        public async Task Delete_DoNotRemoveSourceZipIfOtherFilesFromZipStillExist()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion()
                .Generate();

            var bulkZipFile = _fixture.DefaultFile()
                .WithFilename("data.zip")
                .WithType(BulkDataZip)
                .Generate();

            var subject = _fixture.DefaultSubject()
                .Generate();

            var dataFile = _fixture.DefaultFile()
                .WithFilename("data.csv")
                .WithType(FileType.Data)
                .WithSubjectId(subject.Id)
                .WithSourceId(bulkZipFile.Id)
                .Generate();

            var metaFile = _fixture.DefaultFile()
                .WithFilename("data.meta.csv")
                .WithType(Metadata)
                .WithSubjectId(subject.Id)
                .WithSourceId(bulkZipFile.Id)
                .Generate();

            var releaseDataFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(dataFile)
                .Generate();

            var releaseMetaFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(metaFile)
                .Generate();

            // otherSubject, from the same bulk zip file, will not be deleted
            var otherSubject = _fixture.DefaultSubject()
                .Generate();

            var otherDataFile = _fixture.DefaultFile()
                .WithFilename("other-data.csv")
                .WithType(FileType.Data)
                .WithSubjectId(otherSubject.Id)
                .WithSourceId(bulkZipFile.Id)
                .Generate();

            var otherMetaFile = _fixture.DefaultFile()
                .WithFilename("other-data.meta.csv")
                .WithType(Metadata)
                .WithSubjectId(otherSubject.Id)
                .WithSourceId(bulkZipFile.Id)
                .Generate();

            var otherReleaseDataFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(otherDataFile)
                .Generate();

            var otherReleaseMetaFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(otherMetaFile)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(bulkZipFile, dataFile, metaFile, otherDataFile, otherMetaFile);
                contentDbContext.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile,
                    otherReleaseDataFile, otherReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(dataFile.Id))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(releaseVersion.Id,
                    dataFile.Id,
                    FileType.Data))
                .ReturnsAsync(dataFile);

            privateBlobStorageService.Setup(mock => mock.DeleteBlob(
                    PrivateReleaseFiles, dataFile.Path()))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.DeleteBlob(
                    PrivateReleaseFiles, metaFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.Delete(releaseVersion.Id, dataFile.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(privateBlobStorageService,
                    dataImportService,
                    releaseFileService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(otherReleaseDataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(otherDataFile.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(otherReleaseMetaFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(otherMetaFile.Id));

                // Zip should not be removed since otherReleaseDataFile etc. still exist
                Assert.NotNull(await contentDbContext.Files.FindAsync(bulkZipFile.Id));
            }
        }

        [Fact]
        public async Task DeleteAll()
        {
            var releaseVersion = new ReleaseVersion();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
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
                ReleaseVersion = releaseVersion,
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
                ReleaseVersion = releaseVersion,
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.Files.Add(zipFile);
                contentDbContext.ReleaseFiles.AddRange(ancillaryReleaseFile, chartReleaseFile, dataReleaseFile,
                    metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            dataImportService.Setup(mock => mock.DeleteImport(dataReleaseFile.File.Id))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.DeleteBlob(PrivateReleaseFiles,
                    It.IsIn(dataReleaseFile.Path(), metaReleaseFile.Path(), zipFile.Path())))
                .Returns(Task.CompletedTask);

            releaseFileService.Setup(mock => mock.CheckFileExists(releaseVersion.Id,
                    dataReleaseFile.File.Id,
                    FileType.Data))
                .ReturnsAsync(dataReleaseFile.File);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.DeleteAll(releaseVersion.Id);

                Assert.True(result.IsRight);

                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataReleaseFile.Path()), Times.Once());
                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, metaReleaseFile.Path()), Times.Once());
                privateBlobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(dataReleaseFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(privateBlobStorageService,
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
            var releaseVersion = new ReleaseVersion();

            var amendmentRelease = new ReleaseVersion
            {
                PreviousVersionId = releaseVersion.Id
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
                ReleaseVersion = releaseVersion,
                File = dataFile
            };

            var metaReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = metaFile
            };

            var amendmentReleaseDataFile = new ReleaseFile
            {
                ReleaseVersion = amendmentRelease,
                File = dataFile
            };

            var amendmentReleaseMetaFile = new ReleaseFile
            {
                ReleaseVersion = amendmentRelease,
                File = metaFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendmentRelease);
                contentDbContext.Files.AddRange(zipFile, dataFile, metaFile);
                contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile, amendmentReleaseDataFile,
                    amendmentReleaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService.Setup(mock => mock.CheckFileExists(amendmentRelease.Id,
                    dataFile.Id,
                    FileType.Data))
                .ReturnsAsync(dataFile);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(privateBlobStorageService,
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
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataImportService: dataImportService.Object);

            var result = await service.DeleteAll(Guid.NewGuid());

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(privateBlobStorageService, dataImportService);
        }

        [Fact]
        public async Task DeleteAll_NoFiles()
        {
            var releaseVersion = new ReleaseVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                    privateBlobStorageService: privateBlobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.DeleteAll(releaseVersion.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(privateBlobStorageService, dataImportService);
            }
        }

        [Fact]
        public async Task GetInfo()
        {
            var releaseVersion = new ReleaseVersion();

            var dataReleaseFile = new ReleaseFile
            {
                Name = "Test data",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Filename = "test-data.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                },
                PublicApiDataSetId = Guid.NewGuid(),
                PublicApiDataSetVersion = SemVersion.Parse("1.0.1", SemVersionStyles.Strict)
            };

            var metaReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
                contentDbContext.DataImports.Add(dataImport);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.GetInfo(
                    releaseVersion.Id,
                    dataReleaseFile.FileId
                );

                var fileInfo = result.AssertRight();

                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal("10 Kb", fileInfo.Size);
            }
        }

        [Fact]
        public async Task GetInfo_ReleaseNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // A file for another release exists
                var anotherRelease = new ReleaseVersion();
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        ReleaseVersion = anotherRelease,
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
            var releaseVersion = new ReleaseVersion();
            var otherReleaseVersion = new ReleaseVersion();

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
                contentDbContext.AddRange(
                    new ReleaseFile
                    {
                        ReleaseVersion = releaseVersion,
                        File = dataFile
                    },
                    new ReleaseFile
                    {
                        ReleaseVersion = releaseVersion,
                        File = metaFile
                    }
                );
                contentDbContext.ReleaseVersions.Add(otherReleaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext);

                var result = await service.GetInfo(
                    otherReleaseVersion.Id,
                    dataFile.Id
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetInfo_AmendedRelease()
        {
            var originalReleaseVersion = new ReleaseVersion();
            var amendedReleaseVersion = new ReleaseVersion();

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
                ReleaseVersion = originalReleaseVersion,
                Name = "Test data",
                File = dataFile,
            };

            var metaOriginalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = originalReleaseVersion,
                File = metaFile,
            };

            var dataAmendedReleaseFile = new ReleaseFile
            {
                ReleaseVersion = amendedReleaseVersion,
                Name = "Test data amended",
                File = dataFile,
            };

            var metaAmendedReleaseFile = new ReleaseFile
            {
                ReleaseVersion = amendedReleaseVersion,
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
                contentDbContext.ReleaseVersions.AddRange(originalReleaseVersion, amendedReleaseVersion);
                contentDbContext.ReleaseFiles.AddRange(
                    dataOriginalReleaseFile,
                    dataAmendedReleaseFile,
                    metaOriginalReleaseFile,
                    metaAmendedReleaseFile);
                contentDbContext.DataImports.Add(dataImport);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.GetInfo(
                    amendedReleaseVersion.Id,
                    dataFile.Id
                );

                var fileInfo = result.AssertRight();

                Assert.Equal("Test data amended", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal("10 Kb", fileInfo.Size);
            }
        }

        [Fact]
        public async Task GetAccoutrementsSummary_ReturnsDataBlockAndFootnote()
        {
            var releaseVersion = new ReleaseVersion();
            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    SubjectId = Guid.NewGuid(),
                    Type = FileType.Data,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);
            dataBlockService.Setup(mock => mock.ListDataBlocks(releaseVersion.Id))
                .ReturnsAsync([
                    new DataBlock
                    {
                        Id = Guid.NewGuid(),
                        Name = "DataBlock name!",
                        Query = new FullTableQuery
                        {
                            SubjectId = releaseFile.File.SubjectId.Value,
                        }
                    },
                    new DataBlock
                    {
                        Id = Guid.NewGuid(),
                        Name = "DataBlock for different data set, so shouldn't appear in results!",
                        Query = new FullTableQuery
                        {
                            SubjectId = Guid.NewGuid(),
                        }
                    }
                ]);

            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            footnoteRepository.Setup(mock => mock.GetFootnotes(releaseVersion.Id, releaseFile.File.SubjectId))
                .ReturnsAsync([
                    new Footnote
                    {
                        Id = Guid.NewGuid(),
                        Content = "Footnote content!",
                    },
                ]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await service.GetAccoutrementsSummary(
                    releaseVersionId: releaseVersion.Id,
                    releaseFile.FileId);

                var viewModel = result.AssertRight();

                var dataBlock = Assert.Single(viewModel.DataBlocks);
                Assert.Equal("DataBlock name!", dataBlock.Name);

                var footnote = Assert.Single(viewModel.Footnotes);
                Assert.Equal("Footnote content!", footnote.Content);
            }
        }

        [Fact]
        public async Task GetAccoutrementsSummary_EmptyResult()
        {
            var releaseVersion = new ReleaseVersion();
            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    SubjectId = Guid.NewGuid(),
                    Type = FileType.Data,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);
            dataBlockService.Setup(mock => mock.ListDataBlocks(releaseVersion.Id))
                .ReturnsAsync([]);

            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            footnoteRepository.Setup(mock => mock.GetFootnotes(releaseVersion.Id, releaseFile.File.SubjectId))
                .ReturnsAsync([]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await service.GetAccoutrementsSummary(
                    releaseVersionId: releaseVersion.Id,
                    fileId: releaseFile.FileId);

                var viewModel = result.AssertRight();

                Assert.Empty(viewModel.DataBlocks);
                Assert.Empty(viewModel.Footnotes);
            }
        }

        [Fact]
        public async Task GetAccoutrementsSummary_ReleaseFileNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

            var result = await service.GetAccoutrementsSummary(
                releaseVersionId: Guid.NewGuid(),
                fileId: Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task ReorderDataFiles()
        {
            var releaseVersion = new ReleaseVersion();
            var releaseDataFile1 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 5,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile2 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 3,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile3 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 1,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile4 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 2,
                File = new File
                {
                    Type = FileType.Data,
                },
            };
            var releaseDataFile5 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Order = 0,
                File = new File
                {
                    Type = FileType.Data,
                },
            };

            var releaseMetaFile1 = new ReleaseFile
            {
                ReleaseVersion = releaseDataFile1.ReleaseVersion,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile2 = new ReleaseFile
            {
                ReleaseVersion = releaseDataFile2.ReleaseVersion,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile3 = new ReleaseFile
            {
                ReleaseVersion = releaseDataFile3.ReleaseVersion,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile4 = new ReleaseFile
            {
                ReleaseVersion = releaseDataFile4.ReleaseVersion,
                File = new File
                {
                    Type = FileType.Metadata,
                }
            };
            var releaseMetaFile5 = new ReleaseFile
            {
                ReleaseVersion = releaseDataFile5.ReleaseVersion,
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
                    releaseVersion.Id,
                    [
                        releaseDataFile1.File.Id,
                        releaseDataFile2.File.Id,
                        releaseDataFile3.File.Id,
                        releaseDataFile4.File.Id,
                        releaseDataFile5.File.Id
                    ]);

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
                    .Where(rf => rf.ReleaseVersionId == releaseVersion.Id && rf.File.Type == FileType.Data)
                    .ToList();

                var dbDataFile1 = dbDataFiles.Find(rf => rf.Id == releaseDataFile1.Id);
                Assert.NotNull(dbDataFile1);
                Assert.Equal(0, dbDataFile1.Order);

                var dbDataFile2 = dbDataFiles.Find(rf => rf.Id == releaseDataFile2.Id);
                Assert.NotNull(dbDataFile2);
                Assert.Equal(1, dbDataFile2.Order);

                var dbDataFile3 = dbDataFiles.Find(rf => rf.Id == releaseDataFile3.Id);
                Assert.NotNull(dbDataFile3);
                Assert.Equal(2, dbDataFile3.Order);

                var dbDataFile4 = dbDataFiles.Find(rf => rf.Id == releaseDataFile4.Id);
                Assert.NotNull(dbDataFile4);
                Assert.Equal(3, dbDataFile4.Order);

                var dbDataFile5 = dbDataFiles.Find(rf => rf.Id == releaseDataFile5.Id);
                Assert.NotNull(dbDataFile5);
                Assert.Equal(4, dbDataFile5.Order);

                var dbMetaFiles = contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.ReleaseVersionId == releaseVersion.Id && rf.File.Type == Metadata)
                    .ToList();

                // Non-FileType.Data files should default to Order 0
                var dbMetaFile1 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile1.Id);
                Assert.NotNull(dbMetaFile1);
                Assert.Equal(0, dbMetaFile1.Order);

                var dbMetaFile2 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile2.Id);
                Assert.NotNull(dbMetaFile2);
                Assert.Equal(0, dbMetaFile2.Order);

                var dbMetaFile3 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile3.Id);
                Assert.NotNull(dbMetaFile3);
                Assert.Equal(0, dbMetaFile3.Order);

                var dbMetaFile4 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile4.Id);
                Assert.NotNull(dbMetaFile4);
                Assert.Equal(0, dbMetaFile4.Order);

                var dbMetaFile5 = dbMetaFiles.Find(rf => rf.Id == releaseMetaFile5.Id);
                Assert.NotNull(dbMetaFile5);
                Assert.Equal(0, dbMetaFile5.Order);
            }
        }

        [Fact]
        public async Task ListAll()
        {
            var releaseVersion = new ReleaseVersion();
            var dataReleaseFile1 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                Name = "Test subject 1",
                PublicApiDataSetId = Guid.NewGuid(),
                PublicApiDataSetVersion = SemVersion.Parse("1.0.1", SemVersionStyles.Any),
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
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                }
            };
            var dataReleaseFile2 = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
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
                ReleaseVersion = releaseVersion,
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
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataReleaseFile2,
                    metaReleaseFile2);
                contentDbContext.DataImports.AddRange(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(releaseVersion.Id);

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
                Assert.Equal(dataReleaseFile1.PublicApiDataSetId, files[0].PublicApiDataSetId);
                Assert.Equal(dataReleaseFile1.PublicApiDataSetVersionString, files[0].PublicApiDataSetVersion);

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
                Assert.Null(files[1].PublicApiDataSetId);
                Assert.Null(files[1].PublicApiDataSetVersion);
            }
        }

        [Fact]
        public async Task ListAll_FiltersCorrectly()
        {
            var release1 = new ReleaseVersion();
            var release2 = new ReleaseVersion();

            var dataRelease1File = new ReleaseFile
            {
                ReleaseVersion = release1,
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
                ReleaseVersion = release1,
                File = new File
                {
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                }
            };

            var dataRelease2File = new ReleaseFile
            {
                ReleaseVersion = release2,
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
                ReleaseVersion = release2,
                File = new File
                {
                    Filename = "test-data-2.meta.csv",
                    Type = Metadata
                }
            };

            var ancillaryRelease1File = new ReleaseFile
            {
                ReleaseVersion = release1,
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
                contentDbContext.ReleaseVersions.AddRange(release1, release2);
                contentDbContext.ReleaseFiles.AddRange(
                    dataRelease1File,
                    metaRelease1File,
                    dataRelease2File,
                    metaRelease2File,
                    ancillaryRelease1File); // Not FileType.Data
                contentDbContext.AddRange(dataImports);
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
                Assert.Null(files[0].PublicApiDataSetId);
                Assert.Null(files[0].PublicApiDataSetVersion);
            }
        }

        [Fact]
        public async Task ListAll_AmendedRelease()
        {
            var originalReleaseVersion = new ReleaseVersion();
            var amendedReleaseVersion = new ReleaseVersion();

            var dataReleaseFile1 = new ReleaseFile
            {
                ReleaseVersion = originalReleaseVersion,
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
                ReleaseVersion = originalReleaseVersion,
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
                ReleaseVersion = originalReleaseVersion,
                Name = "Test subject 2",
                File = dataFile2,
            };
            var metaOriginalReleaseFile2 = new ReleaseFile
            {
                ReleaseVersion = originalReleaseVersion,
                File = metaFile2,
            };
            var dataAmendedReleaseFile2 = new ReleaseFile
            {
                ReleaseVersion = amendedReleaseVersion,
                Name = "Test subject 2 name change",
                File = dataFile2,
            };
            var metaAmendedReleaseFile2 = new ReleaseFile
            {
                ReleaseVersion = amendedReleaseVersion,
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
                contentDbContext.ReleaseVersions.AddRange(originalReleaseVersion, amendedReleaseVersion);
                contentDbContext.ReleaseFiles.AddRange(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataOriginalReleaseFile2,
                    metaOriginalReleaseFile2,
                    // Only second data file is attached to amended release
                    dataAmendedReleaseFile2,
                    metaAmendedReleaseFile2);
                contentDbContext.DataImports.AddRange(dataImports);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(amendedReleaseVersion.Id);

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
                Assert.Null(files[0].PublicApiDataSetId);
                Assert.Null(files[0].PublicApiDataSetVersion);
            }
        }

        [Fact]
        public async Task SaveDataSetsFromTemporaryBlobStorage_Success_ReturnsUploadSummary()
        {
            // Arrange
            var dataSetName = "Test Data Set";

            var dataFile = _fixture
                .DefaultFile()
                .WithType(FileType.Data)
                .Generate();

            var metaFile = _fixture.DefaultFile().WithType(FileType.Metadata).Generate();

            var import = new DataImport
            {
                File = dataFile,
                FileId = dataFile.Id,
                MetaFile = metaFile,
                MetaFileId = metaFile.Id,
            };

            var releaseFiles = new List<ReleaseFile>
            {
                _fixture.DefaultReleaseFile().WithFile(dataFile).Generate(),
            };

            ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease()
                    .WithPublication(_fixture.DefaultPublication()));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.DataImports.Add(import);
            await contentDbContext.SaveChangesAsync();

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
            var dataSetFileStorage = new Mock<IDataSetFileStorage>(Strict);

            var dataSetFile = new DataSetUploadResultViewModel
            {
                Title = dataSetName,
                DataFileId = dataFile.Id,
                DataFileName = dataFile.Filename,
                DataFileSize = 434,
                MetaFileId = metaFile.Id,
                MetaFileName = metaFile.Filename,
                MetaFileSize = 157,
                ReplacingFileId = null,
            };

            var dataPath = $"{releaseVersion.Id}/data/{dataSetFile.DataFileId}";
            var metaPath = $"{releaseVersion.Id}/data/{dataSetFile.MetaFileId}";

            privateBlobStorageService.SetupCheckBlobExists(PrivateReleaseTempFiles, dataPath, exists: true);
            privateBlobStorageService.SetupCheckBlobExists(PrivateReleaseTempFiles, metaPath, exists: true);

            dataSetFileStorage
                .Setup(mock => mock.MoveDataSetsToPermanentStorage(
                    It.IsAny<Guid>(),
                    It.IsAny<List<DataSetUploadResultViewModel>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(releaseFiles));

            var service = SetupReleaseDataFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataSetFileStorage: dataSetFileStorage.Object);

            // Act
            var result = await service.SaveDataSetsFromTemporaryBlobStorage(
                releaseVersion.Id,
                [dataSetFile],
                cancellationToken: default);

            // Assert
            MockUtils.VerifyAllMocks(privateBlobStorageService, dataSetFileStorage);

            var files = result.AssertRight();
            var dataFileInfo = files.Single();
            Assert.Equal(releaseFiles[0].Name, dataFileInfo.Name);
            Assert.Equal(dataFile.Id, dataFileInfo.Id);
            Assert.Equal(dataFile.Filename, dataFileInfo.FileName);
            Assert.Equal(metaFile.Id, dataFileInfo.MetaFileId);
            Assert.Equal(metaFile.Filename, dataFileInfo.MetaFileName);
            Assert.Equal(QUEUED, dataFileInfo.Status);
            Assert.Equal(FileType.Data, dataFileInfo.Type);
        }

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPrivateBlobStorageService? privateBlobStorageService = null,
            IDataSetValidator? dataSetValidator = null,
            IFileRepository? fileRepository = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IReleaseFileService? releaseFileService = null,
            IDataImportService? dataImportService = null,
            IUserService? userService = null,
            IDataSetFileStorage? dataSetFileStorage = null,
            IDataBlockService? dataBlockService = null,
            IFootnoteRepository? footnoteRepository = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseDataFileService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
                dataSetValidator ?? Mock.Of<IDataSetValidator>(Strict),
                fileRepository ?? new FileRepository(contentDbContext),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object,
                dataSetFileStorage ?? Mock.Of<IDataSetFileStorage>(Strict),
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(Strict)
            );
        }
    }
}
