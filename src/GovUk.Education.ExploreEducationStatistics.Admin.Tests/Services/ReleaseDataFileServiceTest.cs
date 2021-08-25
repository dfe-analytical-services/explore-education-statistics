using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Content.Model.Topic;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServiceTest
    {
        private readonly User _user = new User
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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(zipFile);
                await contentDbContext.AddRangeAsync(releaseDataFile, releaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.Delete(release.Id, releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseMetaFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(releaseDataFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
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
            }
        }

        [Fact]
        public async Task Delete_MixedCaseFilename()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Data 1.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id
                }
            };

            var releaseMetaFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Data 1.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(releaseDataFile, releaseMetaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            dataImportService.Setup(mock => mock.DeleteImport(releaseDataFile.File.Id))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, It.IsIn(
                        releaseDataFile.Path(),
                        releaseMetaFile.Path())))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    releaseDataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.Delete(release.Id, releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, releaseMetaFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(releaseDataFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseDataFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(releaseDataFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(releaseMetaFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(releaseMetaFile.File.Id));
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

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

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
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
                Assert.Null((await contentDbContext.Files.FindAsync(dataFile.Id)).ReplacedById);
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            blobStorageService
                .Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    dataFile.BatchesPath(),
                    null))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.Delete(amendmentRelease.Id, dataFile.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, metaReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, zipFile.Path()), Times.Once());

                dataImportService.Verify(mock => mock.DeleteImport(dataReleaseFile.File.Id), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            blobStorageService.Setup(mock => mock.DeleteBlobs(
                    PrivateReleaseFiles,
                    dataFile.BatchesPath(), 
                    null))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
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
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object);

                var result = await service.DeleteAll(Guid.NewGuid());

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);
            }
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

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
            var subject = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release = new Release();
            var dataReleaseFile = new ReleaseFile
            {
                Name = "Test data",
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(dataReleaseFile, metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataReleaseFile.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            meta: GetDataFileMetaValues(
                                metaFileName: "test-data.meta.csv",
                                numberOfRows: 200
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(COMPLETE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataReleaseFile.File.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var fileInfo = result.Right;

                Assert.Equal(dataReleaseFile.File.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaReleaseFile.File.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(dataReleaseFile.File.Created, fileInfo.Created);
                Assert.Equal(COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetInfo_MixedCaseFilename()
        {
            var subject = new Subject();

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release = new Release();

            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test data",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Test data 1.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Test data 1.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(dataReleaseFile, metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataReleaseFile.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            // Don't use GetDataFileMetaValues here as it forces the filename to lower case
                            meta: new Dictionary<string, string>
                            {
                                {BlobInfoExtensions.NameKey, "Test data file name"},
                                {BlobInfoExtensions.MetaFileKey, "Test data 1.meta.csv"},
                                {BlobInfoExtensions.NumberOfRowsKey, "200"}
                            }
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(COMPLETE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataReleaseFile.File.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var fileInfo = result.Right;

                Assert.Equal(dataReleaseFile.File.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("Test data 1.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaReleaseFile.File.Id, fileInfo.MetaFileId);
                Assert.Equal("Test data 1.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
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
                            RootPath = Guid.NewGuid(),
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

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task GetInfo_FileNotForRelease()
        {
            var release = new Release();
            var otherRelease = new Release();

            var dataFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data.csv",
                Type = FileType.Data,
                CreatedById = _user.Id
            };
            var metaFile = new File
            {
                RootPath = Guid.NewGuid(),
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

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task GetInfo_NoMatchingBlob()
        {
            var subject = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release = new Release();
            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test data",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    SubjectId = subject.Id,
                    Filename = "test-data.csv",
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
                    RootPath = Guid.NewGuid(),
                    SubjectId = subject.Id,
                    Filename = "test-data.meta.csv",
                    Type = Metadata
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(dataReleaseFile, metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }
            
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(false);

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(STAGE_1);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataReleaseFile.File.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var fileInfo = result.Right;

                Assert.Equal(dataReleaseFile.File.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaReleaseFile.File.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal("0.00 B", fileInfo.Size);
                Assert.Equal(dataReleaseFile.File.Created, fileInfo.Created);
                Assert.Equal(STAGE_1, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetInfo_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data-archive.zip",
                Type = DataZip
            };
            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test data",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.csv",
                    Type = FileType.Data,
                    Source = zipFile,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.meta.csv",
                    Type = Metadata,
                    Source = zipFile,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    zipFile,
                    dataReleaseFile,
                    metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, zipFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, zipFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipFile.Path(),
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: "test-data.meta.csv",
                                numberOfRows: 0
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(PROCESSING_ARCHIVE_FILE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataReleaseFile.File.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var fileInfo = result.Right;

                Assert.Equal(dataReleaseFile.File.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.False(fileInfo.MetaFileId.HasValue);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal(PROCESSING_ARCHIVE_FILE, fileInfo.Status);
                Assert.Equal(dataReleaseFile.File.Created, fileInfo.Created);
                Assert.Equal("1 Mb", fileInfo.Size);
            }
        }

        [Fact]
        public async Task GetInfo_AmendedRelease()
        {
            var subject = new Subject();

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var originalRelease = new Release();
            var amendedRelease = new Release();

            var dataFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Created = DateTime.UtcNow,
                CreatedById = _user.Id
            };
            var metaFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
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

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    dataOriginalReleaseFile,
                    dataAmendedReleaseFile,
                    metaOriginalReleaseFile,
                    metaAmendedReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            meta: GetDataFileMetaValues(
                                metaFileName: "test-data.meta.csv",
                                numberOfRows: 200
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataFile.Id))
                    .ReturnsAsync(COMPLETE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.GetInfo(
                    amendedRelease.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data amended", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal(_user.Email, fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(dataFile.Created, fileInfo.Created);
                Assert.Equal(COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task ListAll()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release = new Release();
            var dataReleaseFile1 = new ReleaseFile
            {
                Release = release,
                Name = "Test subject 1",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.csv",
                    Type = FileType.Data,
                    SubjectId = subject1.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                    SubjectId = subject1.Id
                }
            };
            var dataReleaseFile2 = new ReleaseFile
            {
                Release = release,
                Name = "Test subject 2",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Test data 2.csv",
                    Type = FileType.Data,
                    SubjectId = subject2.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Test data 2.meta.csv",
                    Type = Metadata,
                    SubjectId = subject2.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataReleaseFile2,
                    metaReleaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, It.IsIn(
                        dataReleaseFile1.Path(), dataReleaseFile2.Path())))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataReleaseFile1.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataReleaseFile1.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            GetDataFileMetaValues(
                                metaFileName: "test-data-1.meta.csv",
                                numberOfRows: 200
                            )
                        )
                    );

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataReleaseFile2.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataReleaseFile2.Path(),
                            size: "800 B",
                            contentType: "text/csv",
                            contentLength: 800L,
                            // Don't use GetDataFileMetaValues here as it forces the filename to lower case
                            meta: new Dictionary<string, string>
                            {
                                {BlobInfoExtensions.MetaFileKey, "Test data 2.meta.csv"},
                                {BlobInfoExtensions.NumberOfRowsKey, "400"}
                            }
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile1.File.Id))
                    .ReturnsAsync(COMPLETE);

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile2.File.Id))
                    .ReturnsAsync(STAGE_2);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

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
                Assert.Equal("400 B", files[0].Size);
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
                Assert.Equal("800 B", files[1].Size);
                Assert.Equal(dataReleaseFile2.File.Created, files[1].Created);
                Assert.Equal(STAGE_2, files[1].Status);
            }
        }

        [Fact]
        public async Task ListAll_FiltersCorrectly()
        {
            var subject = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release1 = new Release();
            var release2 = new Release();

            var dataRelease1File = new ReleaseFile
            {
                Release = release1,
                Name = "Test data",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaRelease1File = new ReleaseFile
            {
                Release = release1,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };

            var dataRelease2File = new ReleaseFile
            {
                Release = release2,
                Name = "Test data 2",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-2.csv",
                    Type = FileType.Data,
                    CreatedById = _user.Id
                }
            };
            var metaRelease2File = new ReleaseFile
            {
                Release = release2,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-2.meta.csv",
                    Type = Metadata
                }
            };

            var ancillaryRelease1File = new ReleaseFile
            {
                Release = release1,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-file.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {

                await contentDbContext.AddRangeAsync(
                    dataRelease1File,
                    metaRelease1File,
                    dataRelease2File,
                    metaRelease2File,
                    ancillaryRelease1File); // Not FileType.Data
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>();
            var dataImportService = new Mock<IDataImportService>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataRelease1File.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataRelease1File.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataRelease1File.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            GetDataFileMetaValues(
                                metaFileName: "test-data-1.meta.csv",
                                numberOfRows: 200
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataRelease1File.File.Id))
                    .ReturnsAsync(COMPLETE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.ListAll(release1.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

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
                Assert.Equal("400 B", files[0].Size);
                Assert.Equal(dataRelease1File.File.Created, files[0].Created);
                Assert.Equal(COMPLETE, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_AmendedRelease()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var originalRelease = new Release();
            var amendedRelease = new Release();

            var dataReleaseFile1 = new ReleaseFile
            {
                Release = originalRelease,
                Name = "Test subject 1",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.csv",
                    Type = FileType.Data,
                    SubjectId = subject1.Id,
                    Created = DateTime.UtcNow.AddDays(-1),
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile1 = new ReleaseFile
            {
                Release = originalRelease,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data-1.meta.csv",
                    Type = Metadata,
                    SubjectId = subject1.Id
                }
            };

            var dataFile2 = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data-2.csv",
                Type = FileType.Data,
                SubjectId = subject2.Id,
                Created = DateTime.UtcNow,
                CreatedById = _user.Id
            };
            var metaFile2 = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data-2.meta.csv",
                Type = Metadata,
                SubjectId = subject2.Id
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


            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    dataReleaseFile1,
                    metaReleaseFile1,
                    dataOriginalReleaseFile2,
                    metaOriginalReleaseFile2,
                    // Only second data file is attached to amended release
                    dataAmendedReleaseFile2,
                    metaAmendedReleaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataFile2.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, dataFile2.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile2.Path(),
                            size: "800 B",
                            contentType: "text/csv",
                            contentLength: 800L,
                            GetDataFileMetaValues(
                                metaFileName: "test-data-2.meta.csv",
                                numberOfRows: 400
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataFile2.Id))
                    .ReturnsAsync(STAGE_2);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.ListAll(amendedRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile2.Id, files[0].Id);
                Assert.Equal("Test subject 2 name change", files[0].Name);
                Assert.Equal("test-data-2.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(metaFile2.Id, files[0].MetaFileId);
                Assert.Equal("test-data-2.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(400, files[0].Rows);
                Assert.Equal("800 B", files[0].Size);
                Assert.Equal(dataFile2.Created, files[0].Created);
                Assert.Equal(STAGE_2, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_NoMatchingBlob()
        {
            var subject = new Subject();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var release = new Release();
            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test subject name",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.csv",
                    Type = FileType.Data,
                    SubjectId = subject.Id,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.meta.csv",
                    Type = Metadata,
                    SubjectId = subject.Id
                }
            };


            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(dataReleaseFile, metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(false);

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(STAGE_1);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );
                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var files = result.Right.ToList();

                Assert.Single(files);
                Assert.Equal(dataReleaseFile.File.Id, files[0].Id);
                Assert.Equal("Test subject name", files[0].Name);
                Assert.Equal("test-data.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(metaReleaseFile.File.Id, files[0].MetaFileId);
                Assert.Equal("test-data.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(0, files[0].Rows);
                Assert.Equal("0.00 B", files[0].Size);
                Assert.Equal(dataReleaseFile.File.Created, files[0].Created);
                Assert.Equal(STAGE_1, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-data-archive.zip",
                Type = DataZip
            };
            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test data",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.csv",
                    Type = FileType.Data,
                    Source = zipFile,
                    Created = DateTime.UtcNow,
                    CreatedById = _user.Id
                }
            };
            var metaReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-data.meta.csv",
                    Type = Metadata,
                    Source = zipFile
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    zipFile,
                    dataReleaseFile,
                    metaReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, dataReleaseFile.Path()))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateReleaseFiles, zipFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, zipFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipFile.Path(),
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: "test-data.meta.csv",
                                numberOfRows: 0
                            )
                        )
                    );

                dataImportService
                    .Setup(s => s.GetStatus(dataReleaseFile.File.Id))
                    .ReturnsAsync(PROCESSING_ARCHIVE_FILE);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object
                );

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataImportService);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataReleaseFile.File.Id, files[0].Id);
                Assert.Equal("Test data", files[0].Name);
                Assert.Equal("test-data.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.False(files[0].MetaFileId.HasValue);
                Assert.Equal("test-data.meta.csv", files[0].MetaFileName);
                Assert.Equal(_user.Email, files[0].UserName);
                Assert.Equal(0, files[0].Rows);
                Assert.Equal("1 Mb", files[0].Size);
                Assert.Equal(dataReleaseFile.File.Created, files[0].Created);
                Assert.Equal(PROCESSING_ARCHIVE_FILE, files[0].Status);
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
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        dataFormFile))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path => 
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        dataFormFile,
                        It.Is<IDictionary<string, string>>(metadata =>
                            metadata[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && metadata[BlobInfoExtensions.NumberOfRowsKey] == "2")
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        metaFormFile,
                        null
                    )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, 
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: "data/file/path",
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: metaFileName,
                                numberOfRows: 0
                            )
                        )
                    );

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
                    userName: _user.Email,
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
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Right.Created.Value).Milliseconds, 0, 1500);
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

                Assert.Equal(FileType.Data, dataFile.Type);

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
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);

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
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        dataFormFile))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        dataFormFile,
                        It.Is<IDictionary<string, string>>(metadata =>
                            metadata[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && metadata[BlobInfoExtensions.NumberOfRowsKey] == "2")
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, FileType.Data))),
                        metaFormFile,
                        null
                        )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, 
                            It.Is<string>(path => 
                                path.Contains(FilesPath(release.Id, FileType.Data)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: "data/file/path",
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: metaFileName,
                                numberOfRows: 0
                            )
                        )
                    );

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
                    userName: _user.Email,
                    replacingFileId: originalDataReleaseFile.File.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, dataImportService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(originalDataReleaseFile.Name, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal(_user.Email, result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Right.Created.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Right.Status);
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
                        rs.SubjectId == replacementSubject.Id
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

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);

                Assert.Equal(Metadata, metaFile.Type);

                var releaseFiles = contentDbContext.ReleaseFiles.ToList();

                Assert.Equal(3, releaseFiles.Count);

                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == originalDataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == dataFile.Id));
                Assert.NotNull(releaseFiles.SingleOrDefault(rf =>
                    rf.ReleaseId == release.Id && rf.FileId == metaFile.Id));
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
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(release.Id, zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                dataImportService
                    .Setup(s => s.ImportZip(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        It.Is<File>(file => file.Type == DataZip && file.Filename == zipFileName)))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip))),
                        zipFormFile,
                        It.Is<IDictionary<string, string>>(metadata =>
                            metadata[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && metadata[BlobInfoExtensions.NumberOfRowsKey] == "0")
                    )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles, 
                        It.Is<string>(path => 
                            path.Contains(FilesPath(release.Id, DataZip)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: "zip/file/path",
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: metaFileName,
                                numberOfRows: 0
                            )
                        )
                    );

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
                    userName: _user.Email,
                    subjectName: subjectName);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataArchiveValidationService,
                    fileUploadsValidatorService,
                    dataImportService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(subjectName, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal(_user.Email, result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Right.Created.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Right.Status);
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

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(Metadata, metaFile.Type);

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
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(release.Id, zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                dataImportService
                    .Setup(s => s.ImportZip(
                        It.IsAny<Guid>(),
                        It.Is<File>(file => file.Type == FileType.Data && file.Filename == dataFileName),
                        It.Is<File>(file => file.Type == Metadata && file.Filename == metaFileName),
                        It.Is<File>(file => file.Type == DataZip && file.Filename == zipFileName)))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip))),
                        zipFormFile,
                        It.Is<IDictionary<string, string>>(metadata =>
                            metadata[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && metadata[BlobInfoExtensions.NumberOfRowsKey] == "0")
                    )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, DataZip)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: "zip/file/path",
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                metaFileName: metaFileName,
                                numberOfRows: 0
                            )
                        )
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataImportService: dataImportService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.UploadAsZip(
                    release.Id,
                    zipFormFile,
                    userName: _user.Email,
                    replacingFileId: originalDataReleaseFile.File.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService,
                    dataArchiveValidationService,
                    fileUploadsValidatorService,
                    dataImportService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(originalDataReleaseFile.Name, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal(_user.Email, result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Right.Created.Value).Milliseconds, 0, 1500);
                Assert.Equal(QUEUED, result.Right.Status);
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
                        rs.SubjectId == replacementSubject.Id
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

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(Metadata, metaFile.Type);

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

        private static Mock<IFormFile> CreateFormFileMock(string fileName)
        {
            var formFile = MockFormTestUtils.CreateFormFileMock(fileName);

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + fileName);

            formFile.Setup(f => f.OpenReadStream())
                .Returns(() => System.IO.File.OpenRead(filePath));

            return formFile;
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

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IDataArchiveValidationService dataArchiveValidationService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IFileRepository fileRepository = null,
            IReleaseRepository releaseRepository = null,
            IReleaseFileRepository releaseFileRepository = null,
            IReleaseDataFileRepository releaseDataFileRepository = null,
            IDataImportService dataImportService = null,
            IUserService userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseDataFileService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                releaseRepository ?? new ReleaseRepository(
                    contentDbContext, 
                    statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>()),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
