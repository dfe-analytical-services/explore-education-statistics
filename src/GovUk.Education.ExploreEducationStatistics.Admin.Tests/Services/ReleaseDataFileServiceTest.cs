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
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServiceTest
    {
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
                Release = release,
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var releaseDataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Release = release,
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
                    Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            importService.Setup(mock => mock.RemoveImportTableRowIfExists(release.Id, "data.csv"))
                .Returns(Task.CompletedTask);

            // test that the deletion of the main data and metadata files completed, as well as any zip files that 
            // were uploaded
            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, It.IsIn(
                        releaseDataFile.Path(),
                        releaseMetaFile.Path(),
                        zipFile.Path())))
                .Returns(Task.CompletedTask);

            // set up the returning of batch files, both for this data file being deleted and others not being
            // deleted too
            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(release.Id)))
                .ReturnsAsync(new List<BlobInfo>
                {
                    new BlobInfo($"{AdminReleaseBatchesDirectoryPath(release.Id)}data.csv_000001", "", "", 0, new Dictionary<string, string>()),
                    new BlobInfo($"{AdminReleaseBatchesDirectoryPath(release.Id)}data.csv_000002", "", "", 0, new Dictionary<string, string>()),
                    new BlobInfo($"{AdminReleaseBatchesDirectoryPath(release.Id)}another_data_file.csv_000001", "", "", 0, new Dictionary<string, string>()),
                    new BlobInfo($"{AdminReleaseBatchesDirectoryPath(release.Id)}another_data_file.csv_000002", "", "", 0, new Dictionary<string, string>())
                });
            
            // test that the deletion of any remaining batch files went ahead and that it only affected batch files 
            // for this particular data file
            blobStorageService
                .Setup(mock => 
                    mock.DeleteBlob(PrivateFilesContainerName, $"{AdminReleaseBatchesDirectoryPath(release.Id)}data.csv_000001"))
                .Returns(Task.CompletedTask);
            blobStorageService
                .Setup(mock => 
                    mock.DeleteBlob(PrivateFilesContainerName, $"{AdminReleaseBatchesDirectoryPath(release.Id)}data.csv_000002"))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.Delete(release.Id, releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, releaseDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, releaseMetaFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, zipFile.Path()), Times.Once());

                importService.Verify(mock =>
                    mock.RemoveImportTableRowIfExists(release.Id, "data.csv"), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
                    Release = release,
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
                    Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            importService.Setup(mock => mock.RemoveImportTableRowIfExists(release.Id, "Data 1.csv"))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, It.IsIn(
                        releaseDataFile.Path(),
                        releaseMetaFile.Path())))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(release.Id)))
                .ReturnsAsync(new List<BlobInfo>());
            
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.Delete(release.Id, releaseDataFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, releaseDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, releaseMetaFile.Path()), Times.Once());

                importService.Verify(mock =>
                    mock.RemoveImportTableRowIfExists(release.Id, "Data 1.csv"), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
                Release = release,
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                Release = release,
                Filename = "data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var replacementZipFile = new File
            {
                Release = release,
                Filename = "replacement.zip",
                Type = DataZip,
                SubjectId = replacementSubject.Id
            };

            var replacementDataFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                Release = release,
                SubjectId = replacementSubject.Id,
                Replacing = dataFile,
                Source = replacementZipFile
            };

            dataFile.ReplacedBy = replacementDataFile;

            var replacementMetaFile = new File
            {
                Filename = "replacement.meta.csv",
                Type = Metadata,
                Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            importService.Setup(mock => mock.RemoveImportTableRowIfExists(release.Id, replacementDataFile.Filename))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock => mock.DeleteBlob(PrivateFilesContainerName, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(release.Id)))
                .ReturnsAsync(new List<BlobInfo>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.Delete(release.Id, replacementDataFile.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, replacementDataFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                        mock.DeleteBlob(PrivateFilesContainerName, replacementMetaFile.Path()),
                    Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, replacementZipFile.Path()), Times.Once());

                importService.Verify(mock =>
                    mock.RemoveImportTableRowIfExists(release.Id, replacementDataFile.Filename), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
                Release = release,
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(amendmentRelease.Id)))
                .ReturnsAsync(new List<BlobInfo>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.Delete(amendmentRelease.Id, dataFile.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
                    Type = Ancillary,
                    Release = release
                }
            };

            var chartReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "chart.png",
                    Type = Chart,
                    Release = release
                }
            };

            var zipFile = new File
            {
                Release = release,
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id
            };

            var dataReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Release = release,
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
                    Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            importService.Setup(mock => mock.RemoveImportTableRowIfExists(release.Id, "data.csv"))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock => mock.DeleteBlob(PrivateFilesContainerName, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(release.Id)))
                .ReturnsAsync(new List<BlobInfo>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, dataReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, metaReleaseFile.Path()), Times.Once());
                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, zipFile.Path()), Times.Once());

                importService.Verify(mock =>
                    mock.RemoveImportTableRowIfExists(release.Id, "data.csv"), Times.Once());

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
                Release = release,
                Filename = "data.zip",
                Type = DataZip,
                SubjectId = subject.Id,
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id,
                Source = zipFile
            };

            var metaFile = new File
            {
                Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            blobStorageService
                .Setup(mock =>
                    mock.ListBlobs(PrivateFilesContainerName, AdminReleaseBatchesDirectoryPath(amendmentRelease.Id)))
                .ReturnsAsync(new List<BlobInfo>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.DeleteAll(Guid.NewGuid());

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);

                MockUtils.VerifyAllMocks(blobStorageService, importService);
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseDataFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importService);
            }
        }

        [Fact]
        public async Task GetInfo()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test data",
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "test-data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };
            var metaFile = new File
            {
                Release = release,
                Filename = "test-data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

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
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = IStatus.COMPLETE
                    });

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(dataFile.Path(), fileInfo.Path);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(DateTimeOffset.Parse("2020-09-16T12:00:00Z"), fileInfo.Created);
                Assert.Equal(IStatus.COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetInfo_MixedCaseFilename()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test data"
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "Test data 1.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };
            var metaFile = new File
            {
                Release = release,
                Filename = "Test data 1.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            
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
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile.Path(),
                            size: "400 B",
                            contentType: "text/csv",
                            contentLength: 400L,
                            // Don't use GetDataFileMetaValues here as it forces the filename to lower case
                            meta: new Dictionary<string, string>
                            {
                                {BlobInfoExtensions.NameKey, "Test data file name"},
                                {BlobInfoExtensions.MetaFileKey, "Test data 1.meta.csv"},
                                {BlobInfoExtensions.UserNameKey, "test@test.com"},
                                {BlobInfoExtensions.NumberOfRowsKey, "200"}
                            },
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "Test data 1.csv"))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = IStatus.COMPLETE
                    });

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("Test data 1.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(dataFile.Path(), fileInfo.Path);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("Test data 1.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(DateTimeOffset.Parse("2020-09-16T12:00:00Z"), fileInfo.Created);
                Assert.Equal(IStatus.COMPLETE, fileInfo.Status);
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
                            Release = anotherRelease,
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
                Release = release,
                Filename = "test-data.csv",
                Type = FileType.Data
            };
            var metaFile = new File
            {
                Release = release,
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
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test data"
            };
            var dataFile = new File
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.csv",
                Type = FileType.Data
            };
            var metaFile = new File
            {
                Release = release,
                SubjectId = subject.Id,
                Filename = "test-data.meta.csv",
                Type = Metadata
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

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

                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(false);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal("test-data.csv", fileInfo.Path);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("", fileInfo.UserName);
                Assert.Equal(0, fileInfo.Rows);
                Assert.Equal("0.00 B", fileInfo.Size);
                Assert.Equal(IStatus.NOT_FOUND, fileInfo.Status);
            }
        }

        [Fact]
        public async Task GetInfo_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFile = new File
            {
                Release = release,
                Filename = "test-data-archive.zip",
                Type = DataZip,
            };
            var dataFile = new File
            {
                Release = release,
                Filename = "test-data.csv",
                Type = FileType.Data,
                Source = zipFile
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
                        File = zipFile
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            Release = release,
                            Filename = "test-data.meta.csv",
                            Type = Metadata,
                            Source = zipFile
                        }
                    }
                );

                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, zipFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipFile.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.PROCESSING_ARCHIVE_FILE,
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.GetInfo(
                    release.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
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
        public async Task GetInfo_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test data"
            };

            var dataFile = new File
            {
                Release = originalRelease,
                Filename = "test-data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };
            var metaFile = new File
            {
                Release = originalRelease,
                Filename = "test-data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(dataFile, metaFile);
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = dataFile
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = metaFile
                    }
                );
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        File = dataFile
                    },
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        File = metaFile
                    }
                );

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(originalRelease.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.GetInfo(
                    amendedRelease.Id,
                    dataFile.Id
                );

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var fileInfo = result.Right;

                Assert.Equal(dataFile.Id, fileInfo.Id);
                Assert.Equal("Test data", fileInfo.Name);
                Assert.Equal("test-data.csv", fileInfo.FileName);
                Assert.Equal("csv", fileInfo.Extension);
                Assert.Equal(dataFile.Path(), fileInfo.Path);
                Assert.Equal(metaFile.Id, fileInfo.MetaFileId);
                Assert.Equal("test-data.meta.csv", fileInfo.MetaFileName);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.Equal(200, fileInfo.Rows);
                Assert.Equal("400 B", fileInfo.Size);
                Assert.Equal(DateTimeOffset.Parse("2020-09-16T12:00:00Z"), fileInfo.Created);
                Assert.Equal(IStatus.COMPLETE, fileInfo.Status);
            }
        }

        [Fact]
        public async Task ListAll()
        {
            var release = new Release();
            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject 1"
            };
            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject 2"
            };

            var dataFile1 = new File
            {
                Release = release,
                Filename = "test-data-1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id
            };
            var metaFile1 = new File
            {
                Release = release,
                Filename = "test-data-1.meta.csv",
                Type = Metadata,
                SubjectId = subject1.Id
            };
            var dataFile2 = new File
            {
                Release = release,
                Filename = "Test data 2.csv",
                Type = FileType.Data,
                SubjectId = subject2.Id
            };
            var metaFile2 = new File
            {
                Release = release,
                Filename = "Test data 2.meta.csv",
                Type = Metadata,
                SubjectId = subject2.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = dataFile1
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = metaFile1
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = dataFile2
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = metaFile2
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, It.IsIn(
                        dataFile1.Path(), dataFile2.Path())))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile1.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile1.Path(),
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
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile2.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile2.Path(),
                            size: "800 B",
                            contentType: "text/csv",
                            contentLength: 800L,
                            // Don't use GetDataFileMetaValues here as it forces the filename to lower case
                            meta: new Dictionary<string, string>
                            {
                                {BlobInfoExtensions.NameKey, "Test data file 2"},
                                {BlobInfoExtensions.MetaFileKey, "Test data 2.meta.csv"},
                                {BlobInfoExtensions.UserNameKey, "test2@test.com"},
                                {BlobInfoExtensions.NumberOfRowsKey, "400"}
                            }
                        )
                    );

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data-1.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "Test data 2.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.STAGE_2,
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var files = result.Right.ToList();

                Assert.Equal(2, files.Count);

                Assert.Equal(dataFile1.Id, files[0].Id);
                Assert.Equal("Test subject 1", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile1.Path(), files[0].Path);
                Assert.Equal(metaFile1.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal("test1@test.com", files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("400 B", files[0].Size);
                Assert.Equal(IStatus.COMPLETE, files[0].Status);

                Assert.Equal(dataFile2.Id, files[1].Id);
                Assert.Equal("Test subject 2", files[1].Name);
                Assert.Equal("Test data 2.csv", files[1].FileName);
                Assert.Equal("csv", files[1].Extension);
                Assert.Equal(dataFile2.Path(), files[1].Path);
                Assert.Equal(metaFile2.Id, files[1].MetaFileId);
                Assert.Equal("Test data 2.meta.csv", files[1].MetaFileName);
                Assert.Equal("test2@test.com", files[1].UserName);
                Assert.Equal(400, files[1].Rows);
                Assert.Equal("800 B", files[1].Size);
                Assert.Equal(IStatus.STAGE_2, files[1].Status);
            }
        }

        [Fact]
        public async Task ListAll_FiltersCorrectly()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test data"
            };

            var dataFile = new File
            {
                Release = release,
                Filename = "test-data-1.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };
            var metaFile = new File
            {
                Release = release,
                Filename = "test-data-1.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var otherRelease = new Release();

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
                    },
                    // For other release
                    new ReleaseFile
                    {
                        Release = otherRelease,
                        File = new File
                        {
                            Release = otherRelease,
                            Filename = "test-data-2.csv",
                            Type = FileType.Data
                        }
                    },
                    new ReleaseFile
                    {
                        Release = otherRelease,
                        File = new File
                        {
                            Release = otherRelease,
                            Filename = "test-data-2.meta.csv",
                            Type = Metadata
                        }
                    },
                    // Not the right type
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            Release = release,
                            Filename = "ancillary-file.pdf",
                            Type = Ancillary
                        }
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>();
            var importStatusService = new Mock<IImportStatusService>();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data-1.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.COMPLETE,
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile.Id, files[0].Id);
                Assert.Equal("Test data", files[0].Name);
                Assert.Equal("test-data-1.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile.Path(), files[0].Path);
                Assert.Equal(metaFile.Id, files[0].MetaFileId);
                Assert.Equal("test-data-1.meta.csv", files[0].MetaFileName);
                Assert.Equal("test1@test.com", files[0].UserName);
                Assert.Equal(200, files[0].Rows);
                Assert.Equal("400 B", files[0].Size);
                Assert.Equal(IStatus.COMPLETE, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_AmendedRelease()
        {
            var originalRelease = new Release();
            var amendedRelease = new Release();

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject 1"
            };
            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject 2"
            };

            var dataFile1 = new File
            {
                Release = originalRelease,
                Filename = "test-data-1.csv",
                Type = FileType.Data,
                SubjectId = subject1.Id
            };
            var metaFile1 = new File
            {
                Release = originalRelease,
                Filename = "test-data-1.meta.csv",
                Type = Metadata,
                SubjectId = subject1.Id
            };
            var dataFile2 = new File
            {
                Release = originalRelease,
                Filename = "test-data-2.csv",
                Type = FileType.Data,
                SubjectId = subject2.Id
            };
            var metaFile2 = new File
            {
                Release = originalRelease,
                Filename = "test-data-2.meta.csv",
                Type = Metadata,
                SubjectId = subject2.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = dataFile1
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = metaFile1
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = dataFile2
                    },
                    new ReleaseFile
                    {
                        Release = originalRelease,
                        File = metaFile2
                    }
                );
                // Only second data file is attached to this release
                await contentDbContext.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        File = dataFile2
                    },
                    new ReleaseFile
                    {
                        Release = amendedRelease,
                        File = metaFile2
                    }
                );

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile2.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, dataFile2.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFile2.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(originalRelease.Id, "test-data-2.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.STAGE_2
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.ListAll(amendedRelease.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile2.Id, files[0].Id);
                Assert.Equal("Test subject 2", files[0].Name);
                Assert.Equal("test-data-2.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal(dataFile2.Path(), files[0].Path);
                Assert.Equal(metaFile2.Id, files[0].MetaFileId);
                Assert.Equal("test-data-2.meta.csv", files[0].MetaFileName);
                Assert.Equal("test2@test.com", files[0].UserName);
                Assert.Equal(400, files[0].Rows);
                Assert.Equal("800 B", files[0].Size);
                Assert.Equal(IStatus.STAGE_2, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_NoMatchingBlob()
        {
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Test subject name"
            };
            var dataFile = new File
            {
                Release = release,
                Filename = "test-data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };
            var metaFile = new File
            {
                Release = release,
                Filename = "test-data.meta.csv",
                Type = Metadata,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

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

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(false);

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object
                );
                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService);

                var files = result.Right.ToList();

                Assert.Single(files);
                Assert.Equal(dataFile.Id, files[0].Id);
                Assert.Equal("Test subject name", files[0].Name);
                Assert.Equal("test-data.csv", files[0].FileName);
                Assert.Equal("csv", files[0].Extension);
                Assert.Equal("test-data.csv", files[0].Path);
                Assert.Equal(metaFile.Id, files[0].MetaFileId);
                Assert.Equal("test-data.meta.csv", files[0].MetaFileName);
                Assert.Equal("", files[0].UserName);
                Assert.Equal(0, files[0].Rows);
                Assert.Equal("0.00 B", files[0].Size);
                Assert.Equal(IStatus.NOT_FOUND, files[0].Status);
            }
        }

        [Fact]
        public async Task ListAll_MatchingSourceZipBlob()
        {
            var release = new Release();

            var zipFile = new File
            {
                Release = release,
                Filename = "test-data-archive.zip",
                Type = DataZip
            };
            var dataFile = new File
            {
                Release = release,
                Filename = "test-data.csv",
                Type = FileType.Data,
                Source = zipFile
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
                        File = zipFile
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            Release = release,
                            Filename = "test-data.meta.csv",
                            Type = Metadata,
                            Source = zipFile
                        }
                    }
                );

                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var importStatusService = new Mock<IImportStatusService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, dataFile.Path()))
                    .ReturnsAsync(false);

                blobStorageService
                    .Setup(s => s.CheckBlobExists(PrivateFilesContainerName, zipFile.Path()))
                    .ReturnsAsync(true);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, zipFile.Path()))
                    .ReturnsAsync(
                        new BlobInfo(
                            zipFile.Path(),
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

                importStatusService
                    .Setup(s => s.GetImportStatus(release.Id, "test-data.csv"))
                    .ReturnsAsync(
                        new ImportStatus
                        {
                            Status = IStatus.PROCESSING_ARCHIVE_FILE,
                        }
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    importStatusService: importStatusService.Object
                );

                var result = await service.ListAll(release.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, importStatusService);

                var files = result.Right.ToList();

                Assert.Single(files);

                Assert.Equal(dataFile.Id, files[0].Id);
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
        public async Task Upload()
        {
            const string subjectName = "Test Subject";
            const string dataFileName = "test-data.csv";
            const string metaFileName = "test-data.meta.csv";

            var release = new Release
            {
                ReleaseName = "2000",
                Publication = new Content.Model.Publication
                {
                    Title = "Test publication",
                    Topic = new Content.Model.Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Content.Model.Theme
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

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
                        metaFormFile))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.Import(
                        release.Id,
                        It.IsAny<Guid>(),
                        dataFileName,
                        metaFileName,
                        dataFormFile,
                        false))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path => 
                            path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data))),
                        dataFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == subjectName
                            && options.MetaValues[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && options.MetaValues[BlobInfoExtensions.UserNameKey] == "test@test.com"
                            && options.MetaValues[BlobInfoExtensions.NumberOfRowsKey] == "2")
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data))),
                        metaFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.DataFileKey] == dataFileName)
                    )).Returns(Task.CompletedTask);

                var dataFilePath = AdminReleasePath(release.Id, FileType.Data, Guid.NewGuid());

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, 
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFilePath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                subjectName,
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.Upload(
                    releaseId: release.Id,
                    dataFormFile: dataFormFile,
                    metaFormFile: metaFormFile,
                    userName: "test@test.com",
                    subjectName: subjectName);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, importService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(subjectName, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFilePath, result.Right.Path);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
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
                Assert.Equal(release.Id, dataFile.ReleaseId);

                Assert.Equal(Metadata, metaFile.Type);
                Assert.Equal(release.Id, metaFile.ReleaseId);

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
                Publication = new Content.Model.Publication
                {
                    Title = "Test publication",
                    Topic = new Content.Model.Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Content.Model.Theme
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
                Name = "Test data"
            };

            var originalDataReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Release = release,
                    Filename = "original-data.csv",
                    Type = FileType.Data,
                    SubjectId = originalSubject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext .AddAsync(originalDataReleaseFile);
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataFilesForUpload(
                        release.Id,
                        dataFormFile,
                        metaFormFile))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.Import(
                        release.Id,
                        It.IsAny<Guid>(),
                        dataFileName,
                        metaFileName,
                        dataFormFile,
                        false))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data))),
                        dataFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == originalSubject.Name
                            && options.MetaValues[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && options.MetaValues[BlobInfoExtensions.UserNameKey] == "test@test.com"
                            && options.MetaValues[BlobInfoExtensions.NumberOfRowsKey] == "2")
                    )).Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data))),
                        metaFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.DataFileKey] == dataFileName)
                    )).Returns(Task.CompletedTask);

                var dataFilePath = AdminReleasePath(release.Id, FileType.Data, Guid.NewGuid());

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, 
                            It.Is<string>(path => 
                                path.Contains(AdminReleaseDirectoryPath(release.Id, FileType.Data)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: dataFilePath,
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                originalSubject.Name,
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.Upload(
                    releaseId: release.Id,
                    dataFormFile: dataFormFile,
                    metaFormFile: metaFormFile,
                    userName: "test@test.com",
                    replacingFileId: originalDataReleaseFile.File.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService, importService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(originalSubject.Name, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFilePath, result.Right.Path);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
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
                var fileReferences = contentDbContext.Files.ToList();

                Assert.Equal(3, fileReferences.Count);

                var originalDataFile = fileReferences
                    .Single(f => f.Filename == originalDataReleaseFile.File.Filename);
                var dataFile = fileReferences
                    .Single(f => f.Filename == dataFileName);
                var metaFile = fileReferences
                    .Single(f => f.Filename == metaFileName);

                Assert.Equal(FileType.Data, originalDataFile.Type);
                Assert.Equal(release.Id, originalDataFile.ReleaseId);
                Assert.Equal(dataFile.Id, originalDataFile.ReplacedById);

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(release.Id, dataFile.ReleaseId);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);

                Assert.Equal(Metadata, metaFile.Type);
                Assert.Equal(release.Id, metaFile.ReleaseId);

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
                Publication = new Content.Model.Publication
                {
                    Title = "Test publication",
                    Topic = new Content.Model.Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Content.Model.Theme
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

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

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.Import(
                        release.Id,
                        It.IsAny<Guid>(),
                        dataFileName,
                        metaFileName,
                        zipFormFile,
                        true))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, DataZip))),
                        zipFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == subjectName
                            && options.MetaValues[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && options.MetaValues[BlobInfoExtensions.UserNameKey] == "test@test.com"
                            && options.MetaValues[BlobInfoExtensions.NumberOfRowsKey] == "0")
                    )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName, 
                        It.Is<string>(path => 
                            path.Contains(AdminReleaseDirectoryPath(release.Id, DataZip)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: AdminReleasePath(release.Id, DataZip, Guid.NewGuid()),
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                subjectName,
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.UploadAsZip(
                    releaseId: release.Id,
                    zipFormFile: zipFormFile,
                    userName: "test@test.com",
                    subjectName: subjectName);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataArchiveValidationService, fileUploadsValidatorService,
                    importService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(subjectName, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFileName, result.Right.Path);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var fileReferences = contentDbContext.Files.ToList();

                Assert.Equal(3, fileReferences.Count);

                var dataFile = fileReferences
                    .Single(f => f.Filename == dataFileName);
                var metaFile = fileReferences
                    .Single(f => f.Filename == metaFileName);
                var zipFile = fileReferences
                    .Single(f => f.Filename == zipFileName);

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(release.Id, dataFile.ReleaseId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(Metadata, metaFile.Type);
                Assert.Equal(release.Id, metaFile.ReleaseId);

                Assert.Equal(DataZip, zipFile.Type);
                Assert.Equal(release.Id, zipFile.ReleaseId);

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
                Publication = new Content.Model.Publication
                {
                    Title = "Test publication",
                    Topic = new Content.Model.Topic
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test topic",
                        Theme = new Content.Model.Theme
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
                Name = "Subject name"
            };

            var originalDataReleaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Release = release,
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
            var importService = new Mock<IImportService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                fileUploadsValidatorService
                    .Setup(s => s.ValidateDataArchiveEntriesForUpload(release.Id, archiveFile))
                    .ReturnsAsync(Unit.Instance);

                dataArchiveValidationService
                    .Setup(s => s.ValidateDataArchiveFile(release.Id, zipFormFile))
                    .ReturnsAsync(new Either<ActionResult, IDataArchiveFile>(archiveFile));

                importService
                    .Setup(s => s.CreateImportTableRow(release.Id, dataFileName))
                    .ReturnsAsync(Unit.Instance);

                importService
                    .Setup(s => s.Import(
                        release.Id,
                        It.IsAny<Guid>(),
                        dataFileName,
                        metaFileName,
                        zipFormFile,
                        true))
                    .Returns(Task.CompletedTask);

                blobStorageService.Setup(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, DataZip))),
                        zipFormFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == originalSubject.Name
                            && options.MetaValues[BlobInfoExtensions.MetaFileKey] == metaFileName
                            && options.MetaValues[BlobInfoExtensions.UserNameKey] == "test@test.com"
                            && options.MetaValues[BlobInfoExtensions.NumberOfRowsKey] == "0")
                    )).Returns(Task.CompletedTask);

                blobStorageService
                    .Setup(s => s.GetBlob(PrivateFilesContainerName,
                        It.Is<string>(path =>
                            path.Contains(AdminReleaseDirectoryPath(release.Id, DataZip)))))
                    .ReturnsAsync(
                        new BlobInfo(
                            path: AdminReleasePath(release.Id, DataZip, Guid.NewGuid()),
                            size: "1 Mb",
                            contentType: "application/zip",
                            contentLength: 1000L,
                            meta: GetDataFileMetaValues(
                                name: originalSubject.Name,
                                metaFileName: metaFileName,
                                userName: "test@test.com",
                                numberOfRows: 0
                            ),
                            created: DateTimeOffset.Parse("2020-09-16T12:00:00Z")
                        )
                    );

                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    blobStorageService: blobStorageService.Object,
                    importService: importService.Object,
                    dataArchiveValidationService: dataArchiveValidationService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object
                );

                var result = await service.UploadAsZip(
                    release.Id,
                    zipFormFile,
                    userName: "test@test.com",
                    replacingFileId: originalDataReleaseFile.File.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(blobStorageService, dataArchiveValidationService, fileUploadsValidatorService,
                    importService);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(originalSubject.Name, result.Right.Name);
                Assert.Equal(dataFileName, result.Right.FileName);
                Assert.Equal("csv", result.Right.Extension);
                Assert.Equal(dataFileName, result.Right.Path);
                Assert.True(result.Right.MetaFileId.HasValue);
                Assert.Equal(metaFileName, result.Right.MetaFileName);
                Assert.Equal("test@test.com", result.Right.UserName);
                Assert.Equal(0, result.Right.Rows);
                Assert.Equal("1 Mb", result.Right.Size);
                Assert.Equal(IStatus.QUEUED, result.Right.Status);
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
                var fileReferences = contentDbContext.Files.ToList();

                Assert.Equal(4, fileReferences.Count);

                var originalDataFile = fileReferences
                    .Single(f => f.Filename == originalDataReleaseFile.File.Filename);
                var dataFile = fileReferences
                    .Single(f => f.Filename == dataFileName);
                var metaFile = fileReferences
                    .Single(f => f.Filename == metaFileName);
                var zipFile = fileReferences
                    .Single(f => f.Filename == zipFileName);

                Assert.Equal(FileType.Data, originalDataFile.Type);
                Assert.Equal(release.Id, originalDataFile.ReleaseId);
                Assert.Equal(dataFile.Id, originalDataFile.ReplacedById);
                Assert.Null(originalDataFile.SourceId);

                Assert.Equal(FileType.Data, dataFile.Type);
                Assert.Equal(release.Id, dataFile.ReleaseId);
                Assert.Equal(originalDataFile.Id, dataFile.ReplacingId);
                Assert.Equal(zipFile.Id, dataFile.SourceId);

                Assert.Equal(Metadata, metaFile.Type);
                Assert.Equal(release.Id, metaFile.ReleaseId);

                Assert.Equal(DataZip, zipFile.Type);
                Assert.Equal(release.Id, zipFile.ReleaseId);

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
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName)
                .Returns(fileName);

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

        private static ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IDataArchiveValidationService dataArchiveValidationService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IFileRepository fileRepository = null,
            IReleaseRepository releaseRepository = null,
            IReleaseFileRepository releaseFileRepository = null,
            IImportService importService = null,
            IImportStatusService importStatusService = null,
            IUserService userService = null)
        {
            return new ReleaseDataFileService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                releaseRepository ?? new ReleaseRepository(contentDbContext, statisticsDbContext, Common.Services.MapperUtils.MapperForProfile<MappingProfiles>()),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                importService ?? new Mock<IImportService>().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
