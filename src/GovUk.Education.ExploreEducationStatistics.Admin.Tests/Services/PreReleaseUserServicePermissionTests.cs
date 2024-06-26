#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseUserServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetPreReleaseUsers()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupPreReleaseUserService(
                            userService: userService.Object);
                        return service.GetPreReleaseUsers(_releaseVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupPreReleaseUserService(
                            userService: userService.Object);
                        return service.GetPreReleaseUsersInvitePlan(
                            _releaseVersion.Id,
                            ListOf("test@test.com")
                        );
                    }
                );
        }

        [Fact]
        public async Task InvitePreReleaseUsers()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupPreReleaseUserService(
                            userService: userService.Object);
                        return service.InvitePreReleaseUsers(
                            _releaseVersion.Id,
                            ListOf("test@test.com")
                        );
                    }
                );
        }

        [Fact]
        public async Task RemovePreReleaseUser()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanAssignPreReleaseUsersToSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupPreReleaseUserService(
                            userService: userService.Object);
                        return service.RemovePreReleaseUser(_releaseVersion.Id, "test@test.com");
                    }
                );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion);
        }

        private PreReleaseUserService SetupPreReleaseUserService(
            ContentDbContext? context = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IConfiguration? configuration = null,
            IEmailService? emailService = null,
            IPreReleaseService? preReleaseService = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IUserRepository? userRepository = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            return new(
                context ?? Mock.Of<ContentDbContext>(),
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                configuration ?? Mock.Of<IConfiguration>(Strict),
                emailService ?? Mock.Of<IEmailService>(Strict),
                preReleaseService ?? Mock.Of<IPreReleaseService>(Strict),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? Mock.Of<IUserService>(Strict),
                userRepository ?? Mock.Of<IUserRepository>(Strict),
                userInviteRepository ?? Mock.Of<IUserInviteRepository>(Strict),
                userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
                userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
                httpContextAccessor ?? Mock.Of<IHttpContextAccessor>(Strict)
            );
        }
    }
}
