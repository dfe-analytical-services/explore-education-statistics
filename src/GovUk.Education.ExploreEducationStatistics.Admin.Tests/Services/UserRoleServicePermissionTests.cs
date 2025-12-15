#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class UserRoleServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Publication _publication = new() { Id = Guid.NewGuid() };

    [Fact]
    public async Task SetGlobalRole()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.SetGlobalRole(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            });
    }

    [Fact]
    public async Task AddPublicationRole()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.AddPublicationRole(Guid.NewGuid(), Guid.NewGuid(), Owner);
            });
    }

    [Fact]
    public async Task AddReleaseRole()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var release = publication.Releases.Single();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = SetupUserRoleService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object
                    );

                    return await service.AddReleaseRole(userId: Guid.NewGuid(), releaseId: release.Id, Contributor);
                }
            });
    }

    [Fact]
    public async Task GetAllGlobalRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.GetAllGlobalRoles();
            });
    }

    [Fact]
    public async Task GetAllResourceRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.GetAllResourceRoles();
            });
    }

    [Fact]
    public async Task GetGlobalRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.GetGlobalRoles(Guid.NewGuid().ToString());
            });
    }

    [Fact]
    public async Task GetPublicationRolesForUser()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.GetPublicationRolesForUser(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task GetPublicationRolesForPublication()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                await contentDbContext.AddAsync(_publication);
                await contentDbContext.SaveChangesAsync();

                var service = SetupUserRoleService(contentDbContext: contentDbContext, userService: userService.Object);

                return await service.GetPublicationRolesForPublication(_publication.Id);
            });
    }

    [Fact]
    public async Task GetReleaseRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.GetReleaseRoles(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task RemoveUserPublicationRole()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.RemoveUserPublicationRole(Guid.NewGuid());
            });
    }

    [Fact]
    public async Task RemoveUserReleaseRole()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        UserReleaseRole userReleaseRole = _dataFixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .WithUser(_dataFixture.DefaultUser())
            .WithRole(Contributor);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.All))
            .Returns(new[] { userReleaseRole }.BuildMock());

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                tuple => tuple.Item1.Id == releaseVersion.Release.PublicationId && tuple.Item2 == Contributor,
                CanUpdateSpecificReleaseRole
            )
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(
                    userService: userService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.RemoveUserReleaseRole(userReleaseRole.Id);

                MockUtils.VerifyAllMocks(userService, userReleaseRoleRepository);

                return result;
            });
    }

    [Fact]
    public async Task RemoveAllUserResourceRoles()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .ExpectCheckToFail(CanManageUsersOnSystem)
            .AssertForbidden(async userService =>
            {
                var service = SetupUserRoleService(userService: userService.Object);
                return await service.RemoveAllUserResourceRoles(Guid.NewGuid());
            });
    }

    private static UserRoleService SetupUserRoleService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        UserManager<ApplicationUser>? userManager = null,
        IUserService? userService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new UserRoleService(
            usersAndRolesDbContext,
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict),
            userManager ?? MockUserManager().Object
        );
    }
}
