#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataBlockServicePermissionTests
{
    private static readonly ReleaseVersion ReleaseVersion = new() { Id = Guid.NewGuid() };

    private static readonly DataBlockVersion DataBlockVersion = new()
    {
        Id = Guid.NewGuid(),
        ReleaseVersion = ReleaseVersion,
    };

    [Fact]
    public async Task Get()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.Get(DataBlockVersion.Id);
            });
    }

    [Fact]
    public async Task GetDeletePlan()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.GetDeletePlan(ReleaseVersion.Id, DataBlockVersion.Id);
            });
    }

    [Fact]
    public async Task Create()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.Create(
                    ReleaseVersion.Id,
                    new DataBlockCreateRequest { Heading = "Heading 1", Name = "Name 1" }
                );
            });
    }

    [Fact]
    public async Task Update()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.Update(
                    DataBlockVersion.Id,
                    new DataBlockUpdateRequest { Heading = "Heading 1", Name = "Name 1" }
                );
            });
    }

    [Fact]
    public async Task Delete()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.Delete(ReleaseVersion.Id, DataBlockVersion.Id);
            });
    }

    [Fact]
    public async Task GetUnattachedDataBlocks()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildDataBlockService(userService: userService.Object);
                return service.GetUnattachedDataBlocks(ReleaseVersion.Id);
            });
    }

    private Mock<IPersistenceHelper<ContentDbContext>> PersistenceHelperMock()
    {
        var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
        MockUtils.SetupCall(persistenceHelper, ReleaseVersion.Id, ReleaseVersion);
        MockUtils.SetupCall(persistenceHelper, DataBlockVersion);
        return persistenceHelper;
    }

    private DataBlockService BuildDataBlockService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IReleaseFileService? releaseFileService = null,
        IUserService? userService = null,
        IPrivateBlobCacheService? privateCacheService = null,
        ICacheKeyService? cacheKeyService = null
    )
    {
        var service = new DataBlockService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            persistenceHelper ?? PersistenceHelperMock().Object,
            releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
            userService ?? Mock.Of<IUserService>(Strict),
            AdminMapper(),
            privateCacheService ?? Mock.Of<IPrivateBlobCacheService>(Strict),
            cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict)
        );

        return service;
    }
}
