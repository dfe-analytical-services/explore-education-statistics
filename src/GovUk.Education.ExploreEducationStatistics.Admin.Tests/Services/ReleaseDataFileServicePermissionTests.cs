#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Delete(releaseVersionId: _releaseVersion.Id,
                            fileId: Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public async Task Delete_MultipleFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Delete(releaseVersionId: _releaseVersion.Id,
                            fileIds: new List<Guid>
                        {
                            Guid.NewGuid()
                        });
                    }
                );
        }

        [Fact]
        public async Task DeleteAll()
        {
            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = _releaseVersion,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(
                            contentDbContext: DbUtils.InMemoryApplicationDbContext(contentDbContextId),
                            userService: userService.Object);
                        return service.DeleteAll(_releaseVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task GetInfo()
        {
            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = _releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid()
                }
            };

            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseFile>(releaseFile);

            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(releaseFile.ReleaseVersion, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object,
                            contentPersistenceHelper: persistenceHelper.Object);
                        return service.GetInfo(releaseVersionId: releaseFile.ReleaseVersion.Id,
                            fileId: releaseFile.File.Id);
                    }
                );
        }

        [Fact]
        public async Task ListAll()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.ListAll(_releaseVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task ReorderDataFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.ReorderDataFiles(_releaseVersion.Id, new List<Guid>());
                    }
                );
        }

        [Fact]
        public async Task Upload()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Upload(releaseVersionId: _releaseVersion.Id,
                            dataFormFile: new Mock<IFormFile>().Object,
                            metaFormFile: new Mock<IFormFile>().Object,
                            replacingFileId: null,
                            dataSetTitle: "");
                    }
                );
        }

        [Fact]
        public async Task UploadAsZip()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.UploadAsZip(releaseVersionId: _releaseVersion.Id,
                            zipFormFile: new Mock<IFormFile>().Object,
                            dataSetTitle: "",
                            replacingFileId: null);
                    }
                );
        }

        [Fact]
        public async Task UploadAsBulkZipPlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.UploadAsBulkZipPlan(
                            releaseVersionId: _releaseVersion.Id,
                            bulkZipFormFile: new Mock<IFormFile>().Object);
                    }
                );
        }

        [Fact]
        public async Task UploadAsBulkZip()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.UploadAsBulkZip(
                            releaseVersionId: _releaseVersion.Id,
                            bulkZipFormFile: new Mock<IFormFile>().Object);
                    }
                );
        }

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPrivateBlobStorageService? privateBlobStorageService = null,
            IDataArchiveValidationService? dataArchiveValidationService = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IFileRepository? fileRepository = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null,
            IDataImportService? dataImportService = null,
            IUserService? userService = null)
        {
            contentDbContext ??= new Mock<ContentDbContext>().Object;

            return new ReleaseDataFileService(
                contentDbContext,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                privateBlobStorageService ?? new Mock<IPrivateBlobStorageService>(MockBehavior.Strict).Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>(MockBehavior.Strict).Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>(MockBehavior.Strict).Object,
                fileRepository ?? new FileRepository(contentDbContext),
                releaseVersionRepository ?? new ReleaseVersionRepository(
                    contentDbContext,
                    statisticsDbContext ?? new Mock<StatisticsDbContext>().Object),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseFileService ?? new Mock<IReleaseFileService>(MockBehavior.Strict).Object,
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>(MockBehavior.Strict).Object,
                userService ?? new Mock<IUserService>().Object
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
            MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
            return mock;
        }
    }
}
