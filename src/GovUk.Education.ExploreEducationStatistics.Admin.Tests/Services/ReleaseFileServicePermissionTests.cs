using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServicePermissionTests
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
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.Delete(_release.Id,
                            Guid.NewGuid());
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
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.Delete(_release.Id,
                            new List<Guid>
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
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = Ancillary,
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
                        var service = SetupReleaseFileService(
                            contentDbContext: DbUtils.InMemoryApplicationDbContext(contentDbContextId),
                            userService: userService.Object);
                        return service.DeleteAll(_release.Id);
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
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.ListAll(_release.Id, Ancillary);
                    }
                );
        }

        [Fact]
        public void Stream()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.Stream(_release.Id,
                            Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public void UploadAncillary()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.UploadAncillary(releaseId: _release.Id,
                            formFile: new Mock<IFormFile>().Object,
                            name: "");
                    }
                );
        }

        [Fact]
        public void UploadChart()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.UploadChart(releaseId: _release.Id,
                            formFile: new Mock<IFormFile>().Object,
                            replacingId: null);
                    }
                );
        }

        private ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IFileRepository fileRepository = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IReleaseFileRepository releaseFileRepository = null,
            IUserService userService = null)
        {
            return new ReleaseFileService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
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