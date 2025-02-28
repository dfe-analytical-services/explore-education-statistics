#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
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
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FeaturedTableServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new();

    [Fact]
    public async Task Get()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanViewSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Get(
                        _releaseVersion.Id, Guid.NewGuid()
                    );
                }
            );
    }

    [Fact]
    public async Task List()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanViewSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.List(_releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task Create()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Create(_releaseVersion.Id, new FeaturedTableCreateRequest());
                }
            );
    }

    [Fact]
    public async Task Update()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Update(releaseVersionId: _releaseVersion.Id,
                        dataBlockId: Guid.NewGuid(),
                        new FeaturedTableUpdateRequest());
                }
            );
    }

    [Fact]
    public async Task Delete()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Delete(releaseVersionId: _releaseVersion.Id,
                        dataBlockId: Guid.NewGuid());
                }
            );
    }

    [Fact]
    public async Task Reorder()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.Reorder(_releaseVersion.Id, new List<Guid>());
                }
            );
    }

    private FeaturedTableService SetupService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null)
    {
        return new FeaturedTableService(
            contentDbContext ?? new Mock<ContentDbContext>().Object,
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            userService ?? Mock.Of<IUserService>(),
            AdminMapper()
        );
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
        MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
        return mock;
    }
}
