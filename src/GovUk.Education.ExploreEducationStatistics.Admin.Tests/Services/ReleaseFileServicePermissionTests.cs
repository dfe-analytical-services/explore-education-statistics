#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServicePermissionTests
    {
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetFile()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.GetFile(_release.Id, Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public async Task UpdateDataFileDetails()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.UpdateDataFileDetails(
                            _release.Id,
                            Guid.NewGuid(),
                            new ReleaseDataFileUpdateRequest
                            {
                                Title = "Test title"
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.Delete(_release.Id,Guid.NewGuid());
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
                        var service = SetupReleaseFileService(
                            contentDbContext: DbUtils.InMemoryApplicationDbContext(contentDbContextId),
                            userService: userService.Object);
                        return service.DeleteAll(_release.Id);
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
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.ListAll(_release.Id, Ancillary);
                    }
                );
        }

        [Fact]
        public async Task Stream()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
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
        public async Task UploadAncillary()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.UploadAncillary(
                            releaseId: _release.Id,
                            upload: new ReleaseAncillaryFileUploadRequest
                            {
                                File = Mock.Of<IFormFile>(),
                                Title = "Title",
                                Summary = "Summary",
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateAncillary()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseFileService(userService: userService.Object);
                        return service.UpdateAncillary(
                            releaseId: _release.Id,
                            fileId: Guid.NewGuid(),
                            request: new ReleaseAncillaryFileUpdateRequest
                            {
                                File = Mock.Of<IFormFile>()
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UploadChart()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
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
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPrivateBlobStorageService? privateBlobStorageService = null,
            IFileRepository? fileRepository = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IUserService? userService = null)
        {
            return new ReleaseFileService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(),
                fileRepository ?? new FileRepository(contentDbContext),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(),
                userService ?? Mock.Of<IUserService>()
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
