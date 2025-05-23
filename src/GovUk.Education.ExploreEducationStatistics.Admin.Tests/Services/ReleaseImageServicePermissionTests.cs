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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseImageServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task Upload()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupReleaseImageService(userService: userService.Object);
                        return service.Upload(releaseVersionId: _releaseVersion.Id,
                            formFile: new Mock<IFormFile>().Object);
                    }
                );
        }

        private ReleaseImageService SetupReleaseImageService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPrivateBlobStorageService? privateBlobStorageService = null,
            IFileValidatorService? fileValidatorService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IUserService? userService = null)
        {
            return new ReleaseImageService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(),
                fileValidatorService ?? Mock.Of<IFileValidatorService>(),
                releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(),
                userService ?? Mock.Of<IUserService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id,
                _releaseVersion);
        }
    }
}
