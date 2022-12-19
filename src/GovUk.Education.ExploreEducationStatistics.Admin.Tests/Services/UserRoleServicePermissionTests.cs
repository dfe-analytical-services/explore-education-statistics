#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserRoleServicePermissionTests
    {
        private readonly Publication _publication = new()
        {
            Id = Guid.NewGuid(),
        };
        
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
            var userId = Guid.NewGuid();
            var release = new Release();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    release,
                }
            };

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                    tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                    CanUpdateSpecificReleaseRole)
                .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddRangeAsync(publication);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = SetupUserRoleService(contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return await service.AddReleaseRole(userId, release.Id, Contributor);
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
                    
                    var service = SetupUserRoleService(
                        contentDbContext: contentDbContext,
                        userService: userService.Object);
                        
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
            var release = new Release();
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    release,
                }
            };
            var userReleaseRole = new UserReleaseRole
            {
                Release = release,
                Role = Contributor,
                User = new User(),
            };

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<Tuple<Publication, ReleaseRole>>(
                    tuple => tuple.Item1.Id == publication.Id && tuple.Item2 == Contributor,
                    CanUpdateSpecificReleaseRole)
                .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddRangeAsync(publication, userReleaseRole);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = SetupUserRoleService(contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return await service.RemoveUserReleaseRole(userReleaseRole.Id);
                    }
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
            IEmailTemplateService? emailTemplateService = null,
            IUserPublicationRoleRepository? userPublicationRoleRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            UserManager<ApplicationUser>? userManager = null,
            IUserService? userService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserRoleService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext!),
                emailTemplateService ?? new Mock<IEmailTemplateService>().Object,
                userService ?? new Mock<IUserService>().Object,
                userPublicationRoleRepository ?? new Mock<IUserPublicationRoleRepository>().Object,
                userReleaseRoleRepository ?? new Mock<IUserReleaseRoleRepository>().Object,
                userReleaseInviteRepository ?? new Mock<IUserReleaseInviteRepository>().Object,
                userManager ?? MockUserManager().Object);
        }
    }
}
