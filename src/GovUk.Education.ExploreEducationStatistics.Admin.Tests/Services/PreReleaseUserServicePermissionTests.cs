#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Options;
using Moq;
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
            IEmailService? emailService = null,
            IOptions<AppOptions>? appOptions = null,
            IOptions<NotifyOptions>? notifyOptions = null,
            IPreReleaseService? preReleaseService = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IUserRepository? userRepository = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseRoleAndInviteManager? userReleaseRoleAndInviteManager = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null)
        {
            return new(
                context ?? Mock.Of<ContentDbContext>(),
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                emailService ?? Mock.Of<IEmailService>(Strict),
                appOptions ?? Mock.Of<IOptions<AppOptions>>(Strict),
                notifyOptions ?? Mock.Of<IOptions<NotifyOptions>>(Strict),
                preReleaseService ?? Mock.Of<IPreReleaseService>(Strict),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? Mock.Of<IUserService>(Strict),
                userRepository ?? Mock.Of<IUserRepository>(Strict),
                userInviteRepository ?? Mock.Of<IUserInviteRepository>(Strict),
                userReleaseRoleAndInviteManager ?? Mock.Of<IUserReleaseRoleAndInviteManager>(Strict),
                userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict)
            );
        }
    }
}
