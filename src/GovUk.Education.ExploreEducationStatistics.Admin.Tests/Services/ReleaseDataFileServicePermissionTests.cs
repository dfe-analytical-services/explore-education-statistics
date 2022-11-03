#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
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
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServicePermissionTests
    {
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Delete(_release.Id, Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public async Task Delete_MultipleFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Delete(_release.Id, new List<Guid>
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
                Release = _release,
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
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(
                            contentDbContext: DbUtils.InMemoryApplicationDbContext(contentDbContextId),
                            userService: userService.Object);
                        return service.DeleteAll(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task GetInfo()
        {
            var releaseFile = new ReleaseFile
            {
                Release = _release,
                File = new File
                {
                    Id = Guid.NewGuid()
                }
            };

            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseFile>(releaseFile);

            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(releaseFile.Release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object,
                            contentPersistenceHelper: persistenceHelper.Object);
                        return service.GetInfo(releaseFile.Release.Id, releaseFile.File.Id);
                    }
                );
        }

        [Fact]
        public async Task ListAll()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.ListAll(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task ReorderDataFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.ReorderDataFiles(_release.Id, new List<Guid>());
                    }
                );
        }

        [Fact]
        public async Task Upload()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Upload(releaseId: _release.Id,
                            dataFormFile: new Mock<IFormFile>().Object,
                            metaFormFile: new Mock<IFormFile>().Object,
                            replacingFileId: null,
                            subjectName: "");
                    }
                );
        }

        [Fact]
        public async Task UploadAsZip()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.UploadAsZip(releaseId: _release.Id,
                            zipFormFile: new Mock<IFormFile>().Object,
                            replacingFileId: null,
                            subjectName: "");
                    }
                );
        }

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext? contentDbContext = null,
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
            contentDbContext ??= new Mock<ContentDbContext>().Object;

            return new ReleaseDataFileService(
                contentDbContext,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? new Mock<IBlobStorageService>(MockBehavior.Strict).Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>(MockBehavior.Strict).Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>(MockBehavior.Strict).Object,
                fileRepository ?? new FileRepository(contentDbContext),
                releaseRepository ?? new ReleaseRepository(
                    contentDbContext, 
                    statisticsDbContext ?? new Mock<StatisticsDbContext>().Object, 
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>()),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseFileService ?? new Mock<IReleaseFileService>(MockBehavior.Strict).Object,
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>(MockBehavior.Strict).Object,
                userService ?? new Mock<IUserService>().Object
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            return mock;
        }
    }
}
