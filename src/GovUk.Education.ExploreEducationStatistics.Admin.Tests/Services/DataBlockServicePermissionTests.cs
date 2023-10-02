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
        private static readonly Release Release = new()
        {
            Id = Guid.NewGuid()
        };

        private static readonly DataBlock DataBlock = new()
        {
            Id = Guid.NewGuid(),
            Release = Release
        };

        [Fact]
        public async Task Get()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(Release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Get(DataBlock.Id);
                    });
        }

        [Fact]
        public async Task GetDeletePlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.GetDeletePlan(Release.Id, DataBlock.Id);
                    });
        }

        [Fact]
        public async Task Create()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Create(Release.Id, new DataBlockCreateViewModel());
                    });
        }

        [Fact]
        public async Task Update()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Update(DataBlock.Id, new DataBlockUpdateViewModel());
                    });
        }

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.Delete(Release.Id, DataBlock.Id);
                    });
        }

        [Fact]
        public async Task GetUnattachedDataBlocks()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(Release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildDataBlockService(userService: userService.Object);
                        return service.GetUnattachedDataBlocks(Release.Id);
                    }
                );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> PersistenceHelperMock()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, Release.Id, Release);
            MockUtils.SetupCall<ContentDbContext, ContentBlock>(persistenceHelper, DataBlock);
            return persistenceHelper;
        }

        private DataBlockService BuildDataBlockService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IReleaseFileService? releaseFileService = null,
            IUserService? userService = null,
            IBlobCacheService? cacheService = null,
            ICacheKeyService? cacheKeyService = null)
        {
            var service = new DataBlockService(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? PersistenceHelperMock().Object,
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                userService ?? Mock.Of<IUserService>(Strict),
                AdminMapper(),
                cacheService ?? Mock.Of<IBlobCacheService>(Strict),
                cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict)
            );

            return service;
        }
    }
}
