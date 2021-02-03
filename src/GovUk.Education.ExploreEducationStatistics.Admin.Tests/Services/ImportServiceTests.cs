using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ImportServiceTests
    {
        [Fact]
        public async void CancelImport()
        {
            var release = new Release();

            var file = new File
            {
                Type = FileType.Data
            };

            var import = new Import
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
                await contentDbContext.Imports.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(MockBehavior.Strict);
            var queueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

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
                var service = BuildImportService(contentDbContext: contentDbContext,
                    queueService: queueService.Object,
                    userService: userService.Object);

                var result = await service.CancelImport(release.Id, file.Id);
                Assert.True(result.IsRight);
            }

            MockUtils.VerifyAllMocks(userService, queueService);
        }

        [Fact]
        public async void CancelFileImportButNotAllowed()
        {
            var release = new Release();

            var file = new File
            {
                Type = FileType.Data
            };

            var import = new Import
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
                await contentDbContext.Imports.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(MockBehavior.Strict);
            var queueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            userService
                .Setup(s => s.MatchesPolicy(It.Is<File>(f => f.Id == file.Id),
                    SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildImportService(contentDbContext: contentDbContext,
                    queueService: queueService.Object,
                    userService: userService.Object);

                var result = await service.CancelImport(release.Id, file.Id);
                Assert.True(result.IsLeft);
                Assert.IsType<ForbidResult>(result.Left);
            }

            MockUtils.VerifyAllMocks(userService, queueService);
        }

        [Fact]
        public async Task HasIncompleteImports_NoReleaseFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release.Id);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task HasIncompleteImports_ReleaseHasCompletedImports()
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

            var import1 = new Import
            {
                File = file1,
                Status = ImportStatus.COMPLETE
            };

            var import2 = new Import
            {
                File = file2,
                Status = ImportStatus.COMPLETE
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
                await contentDbContext.Imports.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release.Id);
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

            var import1 = new Import
            {
                File = file1,
                Status = ImportStatus.COMPLETE
            };

            var import2 = new Import
            {
                File = file2,
                Status = ImportStatus.STAGE_1
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
                await contentDbContext.Imports.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildImportService(contentDbContext: contentDbContext);

                var result = await service.HasIncompleteImports(release.Id);
                Assert.True(result);
            }
        }

        private static ImportService BuildImportService(
            ContentDbContext contentDbContext,
            IImportRepository importRepository = null,
            IReleaseFileRepository releaseFileRepository = null,
            IStorageQueueService queueService = null,
            IUserService userService = null)
        {
            return new ImportService(
                contentDbContext,
                importRepository ?? new ImportRepository(contentDbContext),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                queueService ?? new Mock<IStorageQueueService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object);
        }
    }
}