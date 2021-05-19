using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
        [Fact]
        public async Task AddGlobalRole()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserRoleService(userService: userService.Object);
                    return await service.AddGlobalRole(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserRoleService(userService: userService.Object);
                    return await service.AddReleaseRole(Guid.NewGuid(), Guid.NewGuid(), Contributor);
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
        public async Task GetPublicationRoles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserRoleService(userService: userService.Object);
                    return await service.GetPublicationRoles(Guid.NewGuid());
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
        public async Task RemoveGlobalRole()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserRoleService(userService: userService.Object);
                    return await service.RemoveGlobalRole(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanManageUsersOnSystem)
                .AssertForbidden(async userService =>
                {
                    var service = SetupUserRoleService(userService: userService.Object);
                    return await service.RemoveUserReleaseRole(Guid.NewGuid());
                });
        }

        private static UserRoleService SetupUserRoleService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailTemplateService emailTemplateService = null,
            IUserPublicationRoleRepository userPublicationRoleRepository = null,
            IUserReleaseRoleRepository userReleaseRoleRepository = null,
            UserManager<ApplicationUser> userManager = null,
            IUserService userService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserRoleService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>().Object,
                userService ?? new Mock<IUserService>().Object,
                userPublicationRoleRepository ?? new Mock<IUserPublicationRoleRepository>().Object,
                userReleaseRoleRepository ?? new Mock<IUserReleaseRoleRepository>().Object,
                userManager ?? MockUserManager().Object);
        }
    }
}
