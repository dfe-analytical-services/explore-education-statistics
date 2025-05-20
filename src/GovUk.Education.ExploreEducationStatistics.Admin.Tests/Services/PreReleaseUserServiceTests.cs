#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseUserServiceTests
    {
        private readonly DataFixture _dataFixture = new();

        private const string PreReleaseTemplateId = "prerelease-template-id";

        private static readonly DateTime PublishedScheduledStartOfDay =
            new DateTime(2020, 09, 09).AsStartOfDayUtcForTimeZone();

        [Fact]
        public async Task GetPreReleaseUsers()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Add roles for existing users
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.1@test.com"
                        }
                    },
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com"
                        }
                    }
                );

                await context.AddRangeAsync(
                    // Add invites for new users
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    // Existing users may also have invites depending on their state and the release status at the time of being invited
                    // * If they were a new user then an invite will exist
                    // * If the release was draft an invite will exist (since emails are sent on approval based on invites)
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "existing.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();

                Assert.Equal(4, users.Count);
                Assert.Equal("existing.1@test.com", users[0].Email);
                Assert.Equal("existing.2@test.com", users[1].Email);
                Assert.Equal("invited.1@test.com", users[2].Email);
                Assert.Equal("invited.2@test.com", users[3].Email);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsers_OrderedCorrectly()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com",
                        }
                    },
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.1@test.com",
                        }
                    }
                );

                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    },
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();

                Assert.Equal(4, users.Count);
                Assert.Equal("existing.1@test.com", users[0].Email);
                Assert.Equal("existing.2@test.com", users[1].Email);
                Assert.Equal("invited.1@test.com", users[2].Email);
                Assert.Equal("invited.2@test.com", users[3].Email);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsers_FiltersInvalidReleaseUsers()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.Contributor,
                        User = new User
                        {
                            Email = "existing.1@test.com",
                        }
                    },
                    // Different release user
                    new UserReleaseRole
                    {
                        ReleaseVersion = otherReleaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com",
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsers_FiltersInvalidReleaseInvites()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.Contributor,
                    },
                    // Different release
                    new UserReleaseInvite
                    {
                        ReleaseVersion = otherReleaseVersion,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan_Fails_InvalidEmail()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    ListOf(
                        "test1@test.com",
                        "not an email",
                        "test2@test.com")
                );

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan_Fails_NoRelease()
        {
            await using var context = InMemoryApplicationDbContext();
            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();
            var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
            var result = await service.GetPreReleaseUsersInvitePlan(
                Guid.NewGuid(),
                ListOf("test@test.com")
            );

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan_Fails_NoInvitableEmails()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseInvites.AddAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "invited.prerelease@test.com"
                    }
                );

                await context.UserReleaseRoles.AddAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.prerelease.user@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    ListOf(
                        "invited.prerelease@test.com",
                        "existing.prerelease.user@test.com")
                );

                result.AssertBadRequest(NoInvitableEmails);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var existingUsers = new List<User>
            {
                new()
                {
                    Email = "existing.user.1@test.com"
                },
                new()
                {
                    Email = "existing.user.2@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.1@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.2@test.com"
                }
            };

            var existingRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.1@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.AddRange(existingUsers);
                context.UserReleaseInvites.AddRange(existingReleaseInvites);
                context.UserReleaseRoles.AddRange(existingRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    ListOf(
                        "new.user.1@test.com",
                        "new.user.2@test.com",
                        "existing.user.1@test.com",
                        "existing.user.2@test.com",
                        "invited.prerelease.1@test.com",
                        "invited.prerelease.2@test.com",
                        "existing.prerelease.user.1@test.com",
                        "existing.prerelease.user.2@test.com")
                );

                var plan = result.AssertRight();

                Assert.Equal(2, plan.AlreadyAccepted.Count);
                Assert.Equal("existing.prerelease.user.1@test.com", plan.AlreadyAccepted[0]);
                Assert.Equal("existing.prerelease.user.2@test.com", plan.AlreadyAccepted[1]);

                Assert.Equal(2, plan.AlreadyInvited.Count);
                Assert.Equal("invited.prerelease.1@test.com", plan.AlreadyInvited[0]);
                Assert.Equal("invited.prerelease.2@test.com", plan.AlreadyInvited[1]);

                Assert.Equal(4, plan.Invitable.Count);
                Assert.Equal("new.user.1@test.com", plan.Invitable[0]);
                Assert.Equal("new.user.2@test.com", plan.Invitable[1]);
                Assert.Equal("existing.user.1@test.com", plan.Invitable[2]);
                Assert.Equal("existing.user.2@test.com", plan.Invitable[3]);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_Fails_InvalidEmail()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf(
                        "test1@test.com",
                        "not an email",
                        "test2@test.com")
                );

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_Fails_NoRelease()
        {
            await using var context = InMemoryApplicationDbContext();
            await using var userAndRolesDbContext = InMemoryUserAndRolesDbContext();
            var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
            var result = await service.InvitePreReleaseUsers(
                Guid.NewGuid(),
                ListOf("test@test.com")
            );

            result.AssertNotFound();
        }

        [Fact]
        public async Task InvitePreReleaseUsers_Fails_NoInvitableEmails()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseInvites.AddAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "invited.prerelease@test.com"
                    }
                );

                await context.UserReleaseRoles.AddAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.prerelease.user@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf(
                        "invited.prerelease@test.com",
                        "existing.prerelease.user@test.com")
                );

                result.AssertBadRequest(NoInvitableEmails);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesExistingUser_ApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.Add(user);

                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        PreReleaseTemplateId,
                        expectedTemplateValues
                    ),
                    Times.Once
                );

                VerifyAllMocks(emailService, preReleaseService);

                var preReleaseUsers = result.AssertRight();
                var preReleaseUser = Assert.Single(preReleaseUsers);

                Assert.NotNull(preReleaseUser);
                Assert.Equal("test@test.com", preReleaseUser!.Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRole = Assert.Single(context.UserReleaseRoles);

                Assert.NotNull(savedUserReleaseRole);
                Assert.Equal(releaseVersion.Id, savedUserReleaseRole!.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRole.Role);
                Assert.Equal(user.Id, savedUserReleaseRole.UserId);

                Assert.Empty(context.UserReleaseInvites);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_FailsSendingEmail_ExistingUser_ApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.Add(user);

                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                VerifyAllMocks(emailService, preReleaseService);

                result.AssertLeft();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                Assert.Empty(context.UserReleaseRoles);
                Assert.Empty(context.UserReleaseInvites);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesExistingUser_DraftRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.Add(user);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                var preReleaseUsers = result.AssertRight();
                var preReleaseUser = Assert.Single(preReleaseUsers);

                Assert.NotNull(preReleaseUser);
                Assert.Equal("test@test.com", preReleaseUser!.Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRole = Assert.Single(context.UserReleaseRoles);

                Assert.NotNull(savedUserReleaseRole);
                Assert.Equal(releaseVersion.Id, savedUserReleaseRole!.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRole.Role);
                Assert.Equal(user.Id, savedUserReleaseRole.UserId);

                // Release invite is created for an existing user if the release is not approved
                var userReleaseInvite = Assert.Single(context.UserReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(releaseVersion.Id, userReleaseInvite.ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, userReleaseInvite.Role);
                Assert.False(userReleaseInvite.EmailSent); // Email not sent immediately for unapproved releases
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesNewUser_ApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>();

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        PreReleaseTemplateId,
                        expectedTemplateValues
                    ),
                    Times.Once
                );

                VerifyAllMocks(emailService, preReleaseService);

                var preReleaseUsers = result.AssertRight();
                var preReleaseUser = Assert.Single(preReleaseUsers);

                Assert.NotNull(preReleaseUser);
                Assert.Equal("test@test.com", preReleaseUser!.Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseInvite = Assert.Single(context.UserReleaseInvites);

                Assert.NotNull(releaseInvite);
                Assert.Equal(releaseVersion.Id, releaseInvite!.ReleaseVersionId);
                Assert.Equal("test@test.com", releaseInvite.Email);
                Assert.Equal(ReleaseRole.PrereleaseViewer, releaseInvite.Role);
                Assert.True(releaseInvite.EmailSent); // Email sent immediately for approved release
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var userInvite = Assert.Single(userAndRolesDbContext.UserInvites);

                Assert.NotNull(userInvite);
                Assert.Equal("test@test.com", userInvite!.Email);
                Assert.Equal(Role.PrereleaseUser.GetEnumValue(), userInvite.RoleId);
                Assert.False(userInvite.Accepted);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_FailsSendingEmail_NewUser_ApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>();

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                VerifyAllMocks(emailService, preReleaseService);

                result.AssertLeft();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                Assert.Empty(context.UserReleaseInvites);
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                Assert.Empty(userAndRolesDbContext.UserInvites);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesNewUser_DraftRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test@test.com")
                );

                var preReleaseUsers = result.AssertRight();
                var preReleaseUser = Assert.Single(preReleaseUsers);

                Assert.NotNull(preReleaseUser);
                Assert.Equal("test@test.com", preReleaseUser!.Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseInvite = Assert.Single(context.UserReleaseInvites);

                Assert.NotNull(releaseInvite);
                Assert.Equal(releaseVersion.Id, releaseInvite!.ReleaseVersionId);
                Assert.Equal("test@test.com", releaseInvite.Email);
                Assert.Equal(ReleaseRole.PrereleaseViewer, releaseInvite.Role);
                Assert.False(releaseInvite.EmailSent); // Email not sent immediately for unapproved releases
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var userInvite = Assert.Single(userAndRolesDbContext.UserInvites);

                Assert.NotNull(userInvite);
                Assert.Equal("test@test.com", userInvite!.Email);
                Assert.Equal(Role.PrereleaseUser.GetEnumValue(), userInvite.RoleId);
                Assert.False(userInvite.Accepted);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesMultipleUsers_ApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer,
                    EmailSent = true
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer,
                    EmailSent = true
                }
            };

            var existingUsers = new List<User>
            {
                new()
                {
                    Email = "existing.user.1@test.com"
                },
                new()
                {
                    Email = "existing.user.2@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.1@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.2@test.com"
                }
            };

            var existingRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.1@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.AddRange(existingUsers);
                context.UserReleaseInvites.AddRange(existingReleaseInvites);
                context.UserReleaseRoles.AddRange(existingRoles);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValuesNewUser = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    It.IsIn("new.user.1@test.com", "new.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesNewUser
                ))
                .Returns(Unit.Instance);

            var expectedTemplateValuesExistingUser =
                GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    It.IsIn("existing.user.1@test.com", "existing.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesExistingUser
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    emailService: emailService.Object,
                    preReleaseService: preReleaseService.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf(
                        "new.user.1@test.com",
                        "new.user.2@test.com",
                        "existing.user.1@test.com",
                        "existing.user.2@test.com",
                        "invited.prerelease.1@test.com",
                        "invited.prerelease.2@test.com",
                        "existing.prerelease.user.1@test.com",
                        "existing.prerelease.user.2@test.com")
                );

                emailService.Verify(mock => mock.SendEmail(
                    It.IsIn("new.user.1@test.com", "new.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesNewUser
                ), Times.Exactly(2));

                emailService.Verify(mock => mock.SendEmail(
                    It.IsIn("existing.user.1@test.com", "existing.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesExistingUser
                ), Times.Exactly(2));

                VerifyAllMocks(emailService, preReleaseService);

                var preReleaseUsers = result.AssertRight();

                // The addresses that are already invited or accepted should have been ignored
                Assert.Equal(4, preReleaseUsers.Count);

                Assert.Equal("new.user.1@test.com", preReleaseUsers[0].Email);
                Assert.Equal("new.user.2@test.com", preReleaseUsers[1].Email);
                Assert.Equal("existing.user.1@test.com", preReleaseUsers[2].Email);
                Assert.Equal("existing.user.2@test.com", preReleaseUsers[3].Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseInvites = await context.UserReleaseInvites.AsQueryable().ToListAsync();
                Assert.Equal(4, releaseInvites.Count);

                Assert.True(releaseInvites.All(invite => invite.ReleaseVersionId == releaseVersion.Id));
                Assert.True(releaseInvites.All(invite => invite.Role == ReleaseRole.PrereleaseViewer));
                Assert.True(releaseInvites.All(invite =>
                    invite.EmailSent)); // Emails sent immediately for approved releases

                Assert.Equal("invited.prerelease.1@test.com", releaseInvites[0].Email);
                Assert.Equal("invited.prerelease.2@test.com", releaseInvites[1].Email);

                Assert.Equal("new.user.1@test.com", releaseInvites[2].Email);
                Assert.Equal("new.user.2@test.com", releaseInvites[3].Email);

                // Two new role assignments should have been created corresponding with the two accepted invites
                var roles = await context.UserReleaseRoles.AsQueryable().ToListAsync();
                Assert.Equal(4, roles.Count);

                Assert.True(roles.All(role => role.ReleaseVersionId == releaseVersion.Id));
                Assert.True(roles.All(role => role.Role == ReleaseRole.PrereleaseViewer));

                // Existing release roles
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.prerelease.user.1@test.com").Id,
                    roles[0].UserId);
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com").Id,
                    roles[1].UserId);

                // New release roles
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.user.1@test.com").Id, roles[2].UserId);
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.user.2@test.com").Id, roles[3].UserId);
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                // Two new user invites should have been created
                var userInvites = await userAndRolesDbContext.UserInvites.AsQueryable().ToListAsync();
                Assert.Equal(2, userInvites.Count);

                Assert.True(userInvites.All(role => role.RoleId == Role.PrereleaseUser.GetEnumValue()));
                Assert.True(userInvites.All(invite => !invite.Accepted));

                Assert.Equal("new.user.1@test.com", userInvites[0].Email);
                Assert.Equal("new.user.2@test.com", userInvites[1].Email);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesMultipleUsers_DraftRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var existingUsers = new List<User>
            {
                new()
                {
                    Email = "existing.user.1@test.com"
                },
                new()
                {
                    Email = "existing.user.2@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.1@test.com"
                },
                new()
                {
                    Email = "existing.prerelease.user.2@test.com"
                }
            };

            var existingRoles = new List<UserReleaseRole>
            {
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.1@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    ReleaseVersion = releaseVersion,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.Users.AddRange(existingUsers);
                context.UserReleaseInvites.AddRange(existingReleaseInvites);
                context.UserReleaseRoles.AddRange(existingRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf(
                        "new.user.1@test.com",
                        "new.user.2@test.com",
                        "existing.user.1@test.com",
                        "existing.user.2@test.com",
                        "invited.prerelease.1@test.com",
                        "invited.prerelease.2@test.com",
                        "existing.prerelease.user.1@test.com",
                        "existing.prerelease.user.2@test.com")
                );

                var preReleaseUsers = result.AssertRight();
                // The addresses that are already invited or accepted should have been ignored
                Assert.Equal(4, preReleaseUsers.Count);

                Assert.Equal("new.user.1@test.com", preReleaseUsers[0].Email);
                Assert.Equal("new.user.2@test.com", preReleaseUsers[1].Email);
                Assert.Equal("existing.user.1@test.com", preReleaseUsers[2].Email);
                Assert.Equal("existing.user.2@test.com", preReleaseUsers[3].Email);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseInvites = await context.UserReleaseInvites.AsQueryable().ToListAsync();
                Assert.Equal(6, releaseInvites.Count);

                Assert.True(releaseInvites.All(invite => invite.ReleaseVersionId == releaseVersion.Id));
                Assert.True(releaseInvites.All(invite => invite.Role == ReleaseRole.PrereleaseViewer));
                Assert.True(releaseInvites.All(invite =>
                    !invite.EmailSent)); // Emails are not sent for unapproved releases

                Assert.Equal("invited.prerelease.1@test.com", releaseInvites[0].Email);
                Assert.Equal("invited.prerelease.2@test.com", releaseInvites[1].Email);

                Assert.Equal("new.user.1@test.com", releaseInvites[2].Email);
                Assert.Equal("new.user.2@test.com", releaseInvites[3].Email);

                Assert.Equal("existing.user.1@test.com", releaseInvites[4].Email);
                Assert.Equal("existing.user.2@test.com", releaseInvites[5].Email);

                // Two new role assignments should have been created corresponding with the two accepted invites
                var roles = await context.UserReleaseRoles.AsQueryable().ToListAsync();
                Assert.Equal(4, roles.Count);

                Assert.True(roles.All(role => role.ReleaseVersionId == releaseVersion.Id));
                Assert.True(roles.All(role => role.Role == ReleaseRole.PrereleaseViewer));

                // Existing release roles
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.prerelease.user.1@test.com").Id,
                    roles[0].UserId);
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com").Id,
                    roles[1].UserId);

                // New release roles
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.user.1@test.com").Id, roles[2].UserId);
                Assert.Equal(existingUsers.Single(u => u.Email == "existing.user.2@test.com").Id, roles[3].UserId);
            }

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                // Two new user invites should have been created
                var userInvites = await userAndRolesDbContext.UserInvites.AsQueryable().ToListAsync();
                Assert.Equal(2, userInvites.Count);

                Assert.True(userInvites.All(role => role.RoleId == Role.PrereleaseUser.GetEnumValue()));
                Assert.True(userInvites.All(invite => !invite.Accepted));

                Assert.Equal("new.user.1@test.com", userInvites[0].Email);
                Assert.Equal("new.user.2@test.com", userInvites[1].Email);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_Fails_InvalidEmail()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.RemovePreReleaseUser(releaseVersion.Id, "not an email");

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_ReleaseRoleRemoved()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.AddAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();

                await userAndRolesDbContext.AddAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    releaseVersion.Id,
                    "test@test.com"
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRoles = await context.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(savedUserReleaseRoles);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_OtherReleaseRolesNotRemoved()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Belongs to another release
                    new UserReleaseRole
                    {
                        ReleaseVersion = otherReleaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Has a different role on the release
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.Contributor,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();

                await userAndRolesDbContext.AddAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    releaseVersion.Id,
                    "test@test.com"
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRoles = await context.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(2, savedUserReleaseRoles.Count);

                Assert.Equal(otherReleaseVersion.Id, savedUserReleaseRoles[0].ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRoles[0].Role);

                Assert.Equal(releaseVersion.Id, savedUserReleaseRoles[1].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, savedUserReleaseRoles[1].Role);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_AllInvitesRemoved()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                await userAndRolesDbContext.AddAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    releaseVersion.Id,
                    "test@test.com"
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                Assert.Empty(context.UserReleaseInvites);

                // Removes system invite as there are no release invites left
                Assert.Empty(userAndRolesDbContext.UserInvites);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_OtherReleaseInvitesNotRemoved()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for another release
                    new UserReleaseInvite
                    {
                        ReleaseVersion = otherReleaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for a different release role
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.Contributor,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                await userAndRolesDbContext.AddAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Accepted = false,
                        Created = DateTime.UtcNow.AddDays(-1)
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    releaseVersion.Id,
                    "test@test.com"
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var savedUserReleaseInvites = await context.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(2, savedUserReleaseInvites.Count);
                Assert.Equal(otherReleaseVersion.Id, savedUserReleaseInvites[0].ReleaseVersionId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseInvites[0].Role);
                Assert.Equal("test@test.com", savedUserReleaseInvites[0].Email);

                Assert.Equal(releaseVersion.Id, savedUserReleaseInvites[1].ReleaseVersionId);
                Assert.Equal(ReleaseRole.Contributor, savedUserReleaseInvites[1].Role);
                Assert.Equal("test@test.com", savedUserReleaseInvites[1].Email);

                var savedUserInvite = await userAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .SingleAsync();

                Assert.Equal("test@test.com", savedUserInvite.Email);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_AcceptedSystemInviteNotRemoved()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                await userAndRolesDbContext.AddAsync(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        RoleId = Role.PrereleaseUser.GetEnumValue(),
                        Accepted = true
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    releaseVersion.Id,
                    "test@test.com"
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                Assert.Empty(context.UserReleaseInvites);

                var savedUserInvite = Assert.Single(userAndRolesDbContext.UserInvites);
                Assert.NotNull(savedUserInvite);
                Assert.Equal("test@test.com", savedUserInvite!.Email);
            }
        }

        private static Dictionary<string, dynamic> GetExpectedPreReleaseTemplateValues(ReleaseVersion releaseVersion,
            bool newUser)
        {
            return new()
            {
                {"newUser", newUser ? "yes" : "no"},
                {"release name", releaseVersion.Release.Title},
                {"publication name", releaseVersion.Release.Publication.Title},
                {
                    "prerelease link",
                    $"http://localhost/publication/{releaseVersion.Release.PublicationId}/release/{releaseVersion.Id}/prerelease/content"
                },
                {"prerelease day", "Tuesday 08 September 2020"},
                {"prerelease time", "09:30"},
                {"publish day", "Wednesday 09 September 2020"},
                {"publish time", "09:30"}
            };
        }

        private static void SetupGetPrereleaseWindow(Mock<IPreReleaseService> preReleaseService,
            ReleaseVersion releaseVersion)
        {
            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
                .Returns(
                    new PreReleaseWindow
                    {
                        Start = DateTime.Parse("2020-09-08T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                        ScheduledPublishDate = DateTime.Parse("2020-09-09T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal)
                    }
                );
        }

        private static IOptions<AppOptions> DefaultAppOptions()
        {
            return new AppOptions { Url = "http://localhost" }.ToOptionsWrapper();
        }

        private static IOptions<NotifyOptions> DefaultNotifyOptions()
        {
            return new NotifyOptions { PreReleaseTemplateId = PreReleaseTemplateId }.ToOptionsWrapper();
        }

        private static PreReleaseUserService SetupPreReleaseUserService(
            ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext,
            IEmailService? emailService = null,
            IOptions<AppOptions>? appOptions = null,
            IOptions<NotifyOptions>? notifyOptions = null,
            IPreReleaseService? preReleaseService = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IUserRepository? userRepository = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null)
        {
            return new(
                context,
                usersAndRolesDbContext,
                emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
                appOptions ?? DefaultAppOptions(),
                notifyOptions ?? DefaultNotifyOptions(),
                preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                userRepository ?? new UserRepository(context),
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(context),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(context)
            );
        }
    }
}
