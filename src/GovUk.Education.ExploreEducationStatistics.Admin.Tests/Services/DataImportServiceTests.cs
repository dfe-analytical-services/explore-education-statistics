#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataImportServiceTests
    {
        [Fact]
        public async Task CancelImport()
        {
            var release = new Release();

            var file = new File
            {
                Type = FileType.Data
            };

            var import = new DataImport
            {
                File = file
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddAsync(new ReleaseFile
                {
                    Release = release,
                    File = file
                });
                await contentDbContext.DataImports.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var userService = new Mock<IUserService>(Strict);
            var queueService = new Mock<IStorageQueueService>(Strict);

            releaseFileService.Setup(s => s.CheckFileExists(release.Id,
                    file.Id,
                    FileType.Data))
                .ReturnsAsync(file);

            userService
                .Setup(s => s.MatchesPolicy(It.Is<File>(f => f.Id == file.Id),
                    SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(true);

            queueService
                .Setup(s => s.AddMessageAsync(ImportsCancellingQueue,
                    It.Is<CancelImportMessage>(m => m.Id == import.Id)))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext,
                    releaseFileService: releaseFileService.Object,
                    queueService: queueService.Object,
                    userService: userService.Object);

                var result = await service.CancelImport(release.Id, file.Id);
                
                MockUtils.VerifyAllMocks(releaseFileService, userService, queueService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task CancelFileImportButNotAllowed()
        {
            var release = new Release();

            var file = new File
            {
                Type = FileType.Data
            };

            var import = new DataImport
            {
                File = file
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddAsync(new ReleaseFile
                {
                    Release = release,
                    File = file
                });
                await contentDbContext.DataImports.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var userService = new Mock<IUserService>(Strict);
            var queueService = new Mock<IStorageQueueService>(Strict);

            releaseFileService.Setup(s => s.CheckFileExists(release.Id,
                    file.Id,
                    FileType.Data))
                .ReturnsAsync(file);

            userService
                .Setup(s => s.MatchesPolicy(It.Is<File>(f => f.Id == file.Id),
                    SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext,
                    releaseFileService: releaseFileService.Object,
                    queueService: queueService.Object,
                    userService: userService.Object);

                var result = await service.CancelImport(release.Id, file.Id);
                
                MockUtils.VerifyAllMocks(releaseFileService, userService, queueService);
                
                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task HasIncompleteImports_NoReleaseFiles()
        {
            var release1 = new Release();
            var release2 = new Release();

            var release2File1 = new File
            {
                Type = FileType.Data
            };

            // Incomplete imports for other Releases should be ignored

            var release2Import1 = new DataImport
            {
                File = release2File1,
                Status = DataImportStatus.STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release2,
                        File = release2File1
                    });
                await contentDbContext.DataImports.AddRangeAsync(release2Import1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release1.Id);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task HasIncompleteImports_ReleaseHasCompletedImports()
        {
            var release1 = new Release();
            var release2 = new Release();

            var release1File1 = new File
            {
                Type = FileType.Data
            };

            var release1File2 = new File
            {
                Type = FileType.Data
            };

            var release2File1 = new File
            {
                Type = FileType.Data
            };

            var release1Import1 = new DataImport
            {
                File = release1File1,
                Status = DataImportStatus.COMPLETE
            };

            var release1Import2 = new DataImport
            {
                File = release1File2,
                Status = DataImportStatus.COMPLETE
            };

            // Incomplete imports for other Releases should be ignored

            var release2Import1 = new DataImport
            {
                File = release2File1,
                Status = DataImportStatus.STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release1,
                        File = release1File1
                    },
                    new ReleaseFile
                    {
                        Release = release1,
                        File = release1File2
                    },
                    new ReleaseFile
                    {
                        Release = release2,
                        File = release2File1
                    });
                await contentDbContext.DataImports.AddRangeAsync(release1Import1, release1Import2, release2Import1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release1.Id);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task HasIncompleteImports_ReleaseHasIncompleteImports()
        {
            var release = new Release();

            var file1 = new File
            {
                Type = FileType.Data
            };

            var file2 = new File
            {
                Type = FileType.Data
            };

            var import1 = new DataImport
            {
                File = file1,
                Status = DataImportStatus.COMPLETE
            };

            var import2 = new DataImport
            {
                File = file2,
                Status = DataImportStatus.STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = file1
                    },
                    new ReleaseFile
                    {
                        Release = release,
                        File = file2
                    });
                await contentDbContext.DataImports.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release.Id);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task Import()
        {
            var subjectId = Guid.NewGuid();

            var dataFile = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subjectId
            };

            var metaFile = new File
            {
                Filename = "data.meta.csv",
                Type = FileType.Metadata,
                SubjectId = subjectId
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Files.AddRangeAsync(dataFile, metaFile);
            }

            var queueService = new Mock<IStorageQueueService>(Strict);

            queueService.Setup(mock => mock.AddMessageAsync(ImportsPendingQueue, It.IsAny<ImportMessage>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataImportService(contentDbContext: contentDbContext,
                    queueService: queueService.Object);

                var result = await service.Import(subjectId, dataFile, metaFile);

                MockUtils.VerifyAllMocks(queueService);

                Assert.Equal(dataFile.Id, result.FileId);
                Assert.Equal(metaFile.Id, result.MetaFileId);
                Assert.Equal(subjectId, result.SubjectId);
                Assert.Equal(DataImportStatus.QUEUED, result.Status);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            }
        }

        private static DataImportService BuildDataImportService(
            ContentDbContext contentDbContext,
            IDataImportRepository? dataImportRepository = null,
            IReleaseFileService? releaseFileService = null,
            IStorageQueueService? queueService = null,
            IUserService? userService = null)
        {
            return new DataImportService(
                contentDbContext,
                dataImportRepository ?? new DataImportRepository(contentDbContext),
                releaseFileService ?? new Mock<IReleaseFileService>(Strict).Object,
                queueService ?? new Mock<IStorageQueueService>(Strict).Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object);
        }
    }
}
