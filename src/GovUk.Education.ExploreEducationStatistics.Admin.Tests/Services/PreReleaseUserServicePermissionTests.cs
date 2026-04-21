#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseUserServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly ReleaseVersion _releaseVersion;

    public PreReleaseUserServicePermissionTests()
    {
        _releaseVersion = _dataFixture.DefaultReleaseVersion().WithRelease(_dataFixture.DefaultRelease());
    }

    [Fact]
    public async Task GetPreReleaseUsers()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == _releaseVersion.Id,
                CanAssignPreReleaseUsersToSpecificRelease
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(_releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupService(contentDbContext: contentDbContext, userService: userService.Object);
                    var result = await service.GetPreReleaseUsers(_releaseVersion.Id);

                    MockUtils.VerifyAllMocks(userService);

                    return result;
                }
            });
    }

    [Fact]
    public async Task GetPrereleaseRolesForUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(userService: userService.Object);
                return await service.GetPrereleaseRolesForUser(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task GetPreReleaseUsersInvitePlan()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupService(userService: userService.Object);
                return service.GetPreReleaseUsersInvitePlan(_releaseVersion.Id, ListOf("test@test.com"));
            });
    }

    [Fact]
    public async Task GrantPreReleaseAccessForMultipleUsers()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupService(userService: userService.Object);
                return service.GrantPreReleaseAccessForMultipleUsers(_releaseVersion.Id, ListOf("test@test.com"));
            });
    }

    [Fact]
    public async Task GrantPreReleaseAccess()
    {
        User user = _dataFixture.DefaultUser();
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var release = publication.Releases.Single();
        var releaseVersion = release.Versions.Single();

        var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(
                rv => rv.Id == releaseVersion.Id,
                CanAssignPreReleaseUsersToSpecificRelease
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object,
                        userRepository: userRepository.Object
                    );

                    var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                    MockUtils.VerifyAllMocks(userService, userRepository);

                    return result;
                }
            });
    }

    [Fact]
    public async Task RemovePreReleaseRoleByCompositeKey()
    {
        User user = _dataFixture.DefaultUser();

        UserReleaseRole userPreReleaseRole = _dataFixture
            .DefaultUserPrereleaseRole()
            .WithReleaseVersion(_releaseVersion)
            .WithUser(user);

        var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        userRepository
            .Setup(mock => mock.FindUserByEmail(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
        userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, userPreReleaseRole);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(userService =>
            {
                var service = SetupService(
                    userService: userService.Object,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = service.RemovePreReleaseRoleByCompositeKey(_releaseVersion.Id, user.Email);

                MockUtils.VerifyAllMocks(userService, userRepository, userPrereleaseRoleRepository);

                return result;
            });
    }

    [Fact]
    public async Task RemoveUserReleaseRole()
    {
        UserReleaseRole userPreReleaseRole = _dataFixture
            .DefaultUserPrereleaseRole()
            .WithReleaseVersion(_releaseVersion)
            .WithUser(_dataFixture.DefaultUser());

        var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
        userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, userPreReleaseRole);

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
            .AssertForbidden(async userService =>
            {
                var service = SetupService(
                    userService: userService.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.RemovePreReleaseRole(userPreReleaseRole.Id);

                MockUtils.VerifyAllMocks(userService, userPrereleaseRoleRepository);

                return result;
            });
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        return MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion);
    }

    private PreReleaseUserService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserPrereleaseRoleRepository? userPrereleaseRoleRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null
    )
    {
        return new(
            contentDbContext ?? DbUtils.InMemoryApplicationDbContext(),
            usersAndRolesDbContext ?? DbUtils.InMemoryUserAndRolesDbContext(),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userPrereleaseRoleRepository ?? Mock.Of<IUserPrereleaseRoleRepository>(MockBehavior.Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict)
        );
    }
}
