using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServicePermissionTest
    {
        [Fact]
        public void ListAllUsers()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                    {
                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
                        {
                            var userManagementService =
                                BuildUserManagementService(
                                    usersAndRolesDbContext: userAndRolesDbContext,
                                    userService: userService.Object);
                            return await userManagementService.ListAllUsers();
                        }
                    }
                );
        }

        [Fact]
        public void GetUser()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                    {
                        var userId = Guid.NewGuid();
                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        var applicationUser = new ApplicationUser
                        {
                            Id = userId.ToString(),
                            FirstName = "TestFirstName",
                            LastName = "TestLastName"
                        };
                        var role = new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString()
                        };
                        var userRole = new IdentityUserRole<string>
                        {
                            UserId = applicationUser.Id,
                            RoleId = role.Id
                        };
                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(applicationUser);
                            await userAndRolesDbContext.AddAsync(role);
                            await userAndRolesDbContext.AddAsync(userRole);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        var contentDbContextId = Guid.NewGuid().ToString();
                        var publication = new Publication
                        {
                            Id = Guid.NewGuid(),
                            Title = "Test Publication"
                        };
                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            ReleaseName = "2000",
                            PublicationId = publication.Id,
                            Publication = publication
                        };
                        var user = new User
                        {
                            Id = userId,
                            FirstName = applicationUser.FirstName,
                            LastName = applicationUser.LastName,
                            Email = "test@test.com"
                        };
                        var userReleaseRole = new UserReleaseRole
                        {
                            Id = Guid.NewGuid(),
                            User = user,
                            UserId = user.Id,
                            Release = release,
                            ReleaseId = release.Id,
                            Role = Lead,
                        };
                        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                        {
                            await contentDbContext.AddAsync(publication);
                            await contentDbContext.AddAsync(release);
                            await contentDbContext.AddAsync(userReleaseRole);
                            await contentDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext =
                            InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                        {
                            var userManagementService =
                                BuildUserManagementService(
                                    contentDbContext: contentDbContext,
                                    usersAndRolesDbContext: userAndRolesDbContext,
                                    userService: userService.Object);
                            return await userManagementService.GetUser(user.Id);
                        }
                    }
                );
        }

        [Fact]
        public void UpdateUser()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                    {
                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        var userId = Guid.NewGuid().ToString();
                        var user = new ApplicationUser
                        {
                            Id = userId,
                            FirstName = "TestFirstName",
                            LastName = "TestLastName"
                        };
                        var roleOld = new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Old Role"
                        };
                        var roleNew = new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "New Role"
                        };
                        var userRole = new IdentityUserRole<string>
                        {
                            UserId = userId,
                            RoleId = roleOld.Id
                        };

                        await using (var userAndRolesDbContext =
                            InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(user);
                            await userAndRolesDbContext.AddAsync(roleOld);
                            await userAndRolesDbContext.AddAsync(roleNew);
                            await userAndRolesDbContext.AddAsync(userRole);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext =
                            InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            var userManagementService = BuildUserManagementService(
                                usersAndRolesDbContext: userAndRolesDbContext,
                                userService: userService.Object);
                            return await userManagementService.UpdateUser(userId, roleNew.Id);
                        }
                    }
                );
        }

        [Fact]
        public void AddReleaseRole()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    var publication = new Publication
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test Publication"
                    };
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2000",
                        PublicationId = publication.Id,
                        Publication = publication
                    };
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "TestFirstName",
                        LastName = "TestLastName",
                        Email = "test@test.com"
                    };
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddAsync(publication);
                        await contentDbContext.AddAsync(release);
                        await contentDbContext.AddAsync(user);
                        await contentDbContext.SaveChangesAsync();
                    }

                    var userAndRolesContextId = Guid.NewGuid().ToString();
                    var appUser = new ApplicationUser
                    {
                        Id = user.Id.ToString(),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    };
                    await using (var userAndRolesDbContext =
                        InMemoryUserAndRolesDbContext(userAndRolesContextId))
                    {
                        await userAndRolesDbContext.AddAsync(appUser);
                        await userAndRolesDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesContextId))
                    {
                        var persistenceHelper = MockPersistenceHelper<ContentDbContext>();
                        SetupCall(persistenceHelper, release.Id, release);
                        var userManagementService = BuildUserManagementService(
                            contentDbContext: contentDbContext,
                            usersAndRolesDbContext: usersAndRolesDbContext,
                            contentPersistenceHelper: persistenceHelper.Object,
                            userService: userService.Object
                        );
                        return await userManagementService.AddReleaseRole(user.Id, release.Id, Approver);
                    }
                });
        }

        [Fact]
        public void RemoveUserReleaseRole()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    var publication = new Publication
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test Publication"
                    };
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2000",
                        PublicationId = publication.Id,
                        Publication = publication
                    };
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "TestFirstName",
                        LastName = "TestLastName",
                        Email = "test@test.com"
                    };
                    var userReleaseRole = new UserReleaseRole
                    {
                        Id = Guid.NewGuid(),
                        User = user,
                        UserId = user.Id,
                        Release = release,
                        ReleaseId = release.Id,
                        Role = Viewer,
                    };
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddAsync(userReleaseRole);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var persistenceHelper = MockPersistenceHelper<ContentDbContext>();
                        SetupCall(persistenceHelper, userReleaseRole.Id, userReleaseRole);

                        var userManagementService = BuildUserManagementService(
                            contentDbContext: contentDbContext,
                            contentPersistenceHelper: persistenceHelper.Object,
                            userService: userService.Object
                        );
                        return await userManagementService.RemoveUserReleaseRole(userReleaseRole.Id);
                    }
                });
        }

        [Fact]
        public void ListReleases()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                {
                    await using (var contentDbContext = InMemoryApplicationDbContext())
                    {
                        var userManagementService = BuildUserManagementService(
                            contentDbContext: contentDbContext,
                            userService: userService.Object
                        );
                        return await userManagementService.ListReleases();
                    }
                });
        }

        [Fact]
        public void ListRoles()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                {
                    await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext())
                    {
                        var userManagementService = BuildUserManagementService(
                            usersAndRolesDbContext: usersAndRolesDbContext,
                            userService: userService.Object
                        );
                        return await userManagementService.ListRoles();
                    }
                });
        }

        [Fact]
        public void ListReleaseRoles()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(async userService =>
                {
                    var userManagementService = BuildUserManagementService(
                        userService: userService.Object
                    );
                    return await userManagementService.ListReleaseRoles();
                });
        }

        [Fact]
        public void ListPendingInvites()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
                        {
                            var userManagementService = BuildUserManagementService(
                                usersAndRolesDbContext: userAndRolesDbContext,
                                userService: userService.Object);
                            return await userManagementService.ListPendingInvites();
                        }
                    });
        }

        [Fact]
        public void InviteUser()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        var user = new ApplicationUser
                        {
                            FirstName = "TestFirstName",
                            LastName = "TestLastName",
                        };
                        var role = new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString()
                        };
                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        await using (var userAndRolesDbContext =
                            InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(user);
                            await userAndRolesDbContext.AddAsync(role);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            var userManagementService = BuildUserManagementService(
                                usersAndRolesDbContext: userAndRolesDbContext,
                                userService: userService.Object);
                            return await userManagementService.InviteUser(
                                "test@test.com",
                                "Test User",
                                role.Id);
                        }
                    });
        }

        [Fact]
        public void CancelInvite()
        {
            PolicyCheckBuilder()
                .ExpectCheck(SecurityPolicies.CanManageUsersOnSystem)
                .AssertSuccess(
                    async userService =>
                    {
                        var userInvite = new UserInvite
                        {
                            Email = "test@test.com",
                            Accepted = false,
                            Role = new IdentityRole(),
                            RoleId = Guid.Empty.ToString(),
                            Created = DateTime.UtcNow,
                            CreatedBy = "Test creator"
                        };

                        var userAndRolesContextId = Guid.NewGuid().ToString();
                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            await userAndRolesDbContext.AddAsync(userInvite);
                            await userAndRolesDbContext.SaveChangesAsync();
                        }

                        await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesContextId))
                        {
                            var userManagementService =
                                BuildUserManagementService(
                                    usersAndRolesDbContext: userAndRolesDbContext,
                                    userService: userService.Object);
                            return await userManagementService.CancelInvite(userInvite.Email);
                        }
                    });
        }

        private static Mock<IConfiguration> ConfigurationMock()
        {
            var notifyInviteTemplateSection = new Mock<IConfigurationSection>();
            var releaseRoleTemplateSection = new Mock<IConfigurationSection>();
            var adminUriSection = new Mock<IConfigurationSection>();
            var configuration = new Mock<IConfiguration>();

            notifyInviteTemplateSection.Setup(m => m.Value)
                .Returns("the-template-id");

            releaseRoleTemplateSection.Setup(m => m.Value)
                .Returns("notify-release-role-template-id");

            adminUriSection.Setup(m => m.Value)
                .Returns("admin-uri");

            configuration
                .Setup(m => m.GetSection("NotifyInviteTemplateId"))
                .Returns(notifyInviteTemplateSection.Object);

            configuration
                .Setup(m => m.GetSection("NotifyReleaseRoleTemplateId"))
                .Returns(releaseRoleTemplateSection.Object);

            configuration
                .Setup(m => m.GetSection("AdminUri"))
                .Returns(adminUriSection.Object);

            return configuration;
        }

        private static UserManagementService BuildUserManagementService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailService emailService = null,
            IUserPublicationRoleRepository userPublicationRoleRepository = null,
            IUserReleaseRoleRepository userReleaseRoleRepository = null,
            UserManager<ApplicationUser> userManager = null,
            IUserService userService = null,
            IConfiguration configuration = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                userService ?? AlwaysTrueUserService().Object,
                contentDbContext,
                emailService ?? new Mock<IEmailService>().Object,
                configuration ?? ConfigurationMock().Object,
                userPublicationRoleRepository ?? new Mock<IUserPublicationRoleRepository>().Object,
                userReleaseRoleRepository ?? new Mock<IUserReleaseRoleRepository>().Object,
                userManager ?? MockUserManager().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext)
            );
        }
    }
}
