#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FeaturedTableServicePermissionTests
{
    private readonly Release _release = new();

    [Fact]
    public async Task Get()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanViewSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Get(
                        _release.Id, Guid.NewGuid()
                    );
                }
            );
    }

    [Fact]
    public async Task List()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanViewSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.List(_release.Id);
                }
            );
    }

    [Fact]
    public async Task Create()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Create(_release.Id, new FeaturedTableCreateRequest());
                }
            );
    }

    [Fact]
    public async Task Update()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Update(_release.Id, Guid.NewGuid(), new FeaturedTableUpdateRequest());
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
                    var service = SetupService(userService: userService.Object);
                    return service.Delete(_release.Id, Guid.NewGuid());
                }
            );
    }

    [Fact]
    public async Task Reorder()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Reorder(_release.Id, new List<Guid>());
                }
            );
    }

    private FeaturedTableService SetupService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IDataBlockService? dataBlockService = null,
        IUserService? userService = null)
    {
        return new FeaturedTableService(
            contentDbContext ?? new Mock<ContentDbContext>().Object,
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            dataBlockService ?? Mock.Of<IDataBlockService>(),
            userService ?? Mock.Of<IUserService>(),
            AdminMapper()
        );
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
        MockUtils.SetupCall(mock, _release.Id, _release);
        return mock;
    }
}
