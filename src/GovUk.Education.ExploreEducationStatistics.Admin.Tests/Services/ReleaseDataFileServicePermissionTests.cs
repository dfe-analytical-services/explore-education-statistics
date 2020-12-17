using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseDataFileServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void Delete()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Delete(_release.Id, Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public void Delete_MultipleFiles()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
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
        public void DeleteAll()
        {
            var releaseFile = new ReleaseFile
            {
                Release = _release,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary,
                    Release = _release
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.AddAsync(releaseFile);
                contentDbContext.SaveChangesAsync();
            }

            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
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
        public void GetInfo()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.GetInfo(_release.Id, Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public void ListAll()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.ListAll(_release.Id);
                    }
                );
        }

        [Fact]
        public void Upload()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.Upload(releaseId: _release.Id,
                            dataFormFile: new Mock<IFormFile>().Object,
                            metaFormFile: new Mock<IFormFile>().Object,
                            userName: "",
                            replacingFileId: null,
                            subjectName: "");
                    }
                );
        }

        [Fact]
        public void UploadAsZip()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseDataFileService(userService: userService.Object);
                        return service.UploadAsZip(releaseId: _release.Id,
                            zipFormFile: new Mock<IFormFile>().Object,
                            userName: "",
                            replacingFileId: null,
                            subjectName: "");
                    }
                );
        }

        private ReleaseDataFileService SetupReleaseDataFileService(
            ContentDbContext contentDbContext = null,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IDataArchiveValidationService dataArchiveValidationService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IFileRepository fileRepository = null,
            IReleaseFileRepository releaseFileRepository = null,
            IImportService importService = null,
            IImportStatusService importStatusService = null,
            IUserService userService = null)
        {
            return new ReleaseDataFileService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                dataArchiveValidationService ?? new Mock<IDataArchiveValidationService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                importService ?? new Mock<IImportService>().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object,
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