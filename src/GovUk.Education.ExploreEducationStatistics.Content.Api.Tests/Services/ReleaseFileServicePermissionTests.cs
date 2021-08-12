#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class ReleaseFileServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task ZipFilesToStream()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseFileService(userService: userService.Object);
                        return service.ZipFilesToStream(
                            releaseId: _release.Id,
                            fileIds: ListOf(Guid.NewGuid()),
                            outputStream: Stream.Null
                        );
                    }
                );
        }

        public ReleaseFileService BuildReleaseFileService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                userService ?? Mock.Of<IUserService>(),
                Mock.Of<ILogger<ReleaseFileService>>()
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