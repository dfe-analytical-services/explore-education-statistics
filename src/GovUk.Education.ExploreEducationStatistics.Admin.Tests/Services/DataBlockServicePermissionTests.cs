using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        private readonly DataBlock _dataBlock = new DataBlock
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void Get()
        {
            PermissionTestUtils.PolicyCheckBuilder<ContentSecurityPolicies>()
                .ExpectResourceCheckToFail(_release, ContentSecurityPolicies.CanViewRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService.Object);
                        return service.Get(_dataBlock.Id);
                    });
        }

        [Fact]
        public void GetDeletePlan()
        {
            PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService.Object);
                        return service.GetDeletePlan(_release.Id, _dataBlock.Id);
                    });
        }

        [Fact]
        public void Create()
        {
            PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService.Object);
                        return service.Create(_release.Id, new DataBlockCreateViewModel());
                    });
        }

        [Fact]
        public void Update()
        {
            PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService.Object);
                        return service.Update(_dataBlock.Id, new DataBlockUpdateViewModel());
                    });
        }

        [Fact]
        public void Delete()
        {
            PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService.Object);
                        return service.Delete(_release.Id, _dataBlock.Id);
                    });
        }

        private Mock<IPersistenceHelper<ContentDbContext>> PersistenceHelperMock()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();

            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);
            MockUtils.SetupCall(persistenceHelper, _dataBlock.Id, _dataBlock);
            MockUtils.SetupCall(
                persistenceHelper,
                new ReleaseContentBlock
                {
                    Release = _release,
                    ReleaseId = _release.Id,
                    ContentBlock = _dataBlock,
                    ContentBlockId = _dataBlock.Id,
                }
            );

            return persistenceHelper;
        }

        private DataBlockService BuildDataBlockService(
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseFileService releaseFileService = null)
        {
            using var context = DbUtils.InMemoryApplicationDbContext();

            var service = new DataBlockService(
                context,
                AdminMapper(),
                persistenceHelper ?? PersistenceHelperMock().Object,
                userService,
                releaseFileService ?? new Mock<IReleaseFileService>().Object
            );

            return service;
        }
    }
}