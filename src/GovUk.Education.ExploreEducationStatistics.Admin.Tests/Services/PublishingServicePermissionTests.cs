using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublishingServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = new Guid("af032e3c-67c2-4562-9717-9a305a468263"),
            ApprovalStatus = ReleaseApprovalStatus.Approved,
            Version = 0,
            PreviousVersionId = new Guid("af032e3c-67c2-4562-9717-9a305a468263")
        };

        [Fact]
        public void RetryReleasePublishing()
        {
            var mocks = Mocks();
            var publishingService = BuildPublishingService(mocks);

            AssertSecurityPoliciesChecked(service =>
                    service.RetryReleasePublishing(_release.Id),
                _release,
                mocks.UserService,
                publishingService,
                CanPublishSpecificRelease);
        }

        private PublishingService BuildPublishingService((Mock<IStorageQueueService> storageQueueService,
            Mock<IUserService> userService,
            Mock<ILogger<PublishingService>> logger) mocks)
        {
            var (storageQueueService, userService, logger) = mocks;

            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);

            return new PublishingService(persistenceHelper.Object,
                storageQueueService.Object,
                userService.Object,
                logger.Object);
        }

        private static (Mock<IStorageQueueService> StorageQueueService,
            Mock<IUserService> UserService,
            Mock<ILogger<PublishingService>> Logger) Mocks()
        {
            var mockConf = new Mock<IConfiguration>();
            mockConf.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);

            return (
                new Mock<IStorageQueueService>(),
                MockUtils.AlwaysTrueUserService(),
                new Mock<ILogger<PublishingService>>());
        }
    }
}