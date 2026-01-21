#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseUserServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new() { Id = Guid.NewGuid() };

    [Fact]
    public async Task GetPreReleaseUsers()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupPreReleaseUserService(userService: userService.Object);
                return service.GetPreReleaseUsers(_releaseVersion.Id);
            });
    }

    [Fact]
    public async Task GetPreReleaseUsersInvitePlan()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupPreReleaseUserService(userService: userService.Object);
                return service.GetPreReleaseUsersInvitePlan(_releaseVersion.Id, ListOf("test@test.com"));
            });
    }

    [Fact]
    public async Task InvitePreReleaseUsers()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupPreReleaseUserService(userService: userService.Object);
                return service.InvitePreReleaseUsers(_releaseVersion.Id, ListOf("test@test.com"));
            });
    }

    [Fact]
    public async Task RemovePreReleaseUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupPreReleaseUserService(userService: userService.Object);
                return service.RemovePreReleaseUser(_releaseVersion.Id, "test@test.com");
            });
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        return MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion);
    }

    private PreReleaseUserService SetupPreReleaseUserService(
        ContentDbContext? context = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        return new(
            context ?? Mock.Of<ContentDbContext>(),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(Strict),
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            userService ?? Mock.Of<IUserService>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict)
        );
    }
}
