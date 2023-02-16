#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

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
        public async Task Get()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Get(_dataBlock.Id);
                    });
        }

        [Fact]
        public async Task GetDeletePlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.GetDeletePlan(_release.Id, _dataBlock.Id);
                    });
        }

        [Fact]
        public async Task Create()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Create(_release.Id, new DataBlockCreateViewModel());
                    });
        }

        [Fact]
        public async Task Update()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Update(_dataBlock.Id, new DataBlockUpdateViewModel());
                    });
        }

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
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

        [Fact]
        public async Task GetUnattachedDataBlocks()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.GetUnattachedDataBlocks(_release.Id);
                    }
                );
        }

        private DataBlockService BuildDataBlockService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseContentBlockRepository? releaseContentBlockRepository = null,
            IUserService? userService = null,
            IBlobCacheService? cacheService = null,
            ICacheKeyService? cacheKeyService = null)
        {
            var service = new DataBlockService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? PersistenceHelperMock().Object,
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                releaseContentBlockRepository ?? Mock.Of<IReleaseContentBlockRepository>(Strict),
                userService ?? Mock.Of<IUserService>(Strict),
                AdminMapper(),
                cacheService ?? Mock.Of<IBlobCacheService>(Strict),
                cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict)
            );

            return service;
        }
    }
}
