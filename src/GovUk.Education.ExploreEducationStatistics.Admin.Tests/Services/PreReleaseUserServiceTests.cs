#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseUserServiceTests
    {
        private const string PreReleaseTemplateId = "prerelease-template-id";

        private static readonly DateTime PublishedScheduledStartOfDay =
            new DateTime(2020, 09, 09).AsStartOfDayUtcForTimeZone();

        [Fact]
        public async Task GetPreReleaseUsers()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Add roles for existing users
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.1@test.com"
                        }
                    },
                    new UserReleaseRole
                    {
                        Release = release,
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
                        Release = release,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    // Existing users may also have invites depending on their state and the release status at the time of being invited
                    // * If they were a new user then an invite will exist
                    // * If the release was draft an invite will exist (since emails are sent on approval based on invites)
                    new UserReleaseInvite
                    {
                        Release = release,
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
                var result = await service.GetPreReleaseUsers(release.Id);

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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com",
                        }
                    },
                    new UserReleaseRole
                    {
                        Release = release,
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
                        Release = release,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    },
                    new UserReleaseInvite
                    {
                        Release = release,
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
                var result = await service.GetPreReleaseUsers(release.Id);

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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
                        User = new User
                        {
                            Email = "existing.1@test.com",
                        }
                    },
                    // Different release user
                    new UserReleaseRole
                    {
                        Release = new Release(),
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
                var result = await service.GetPreReleaseUsers(release.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsers_FiltersInvalidReleaseInvites()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.Contributor,
                    },
                    // Different release
                    new UserReleaseInvite
                    {
                        Release = new Release(),
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
                var result = await service.GetPreReleaseUsers(release.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }

        [Fact]
        public async Task GetPreReleaseUsersInvitePlan_Fails_InvalidEmail()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsersInvitePlan(
                    release.Id,
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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseInvites.AddAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "invited.prerelease@test.com"
                    }
                );

                await context.UserReleaseRoles.AddAsync(
                    new UserReleaseRole
                    {
                        Release = release,
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
                    release.Id,
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
            var release = new Release();

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    Release = release,
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
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Releases.AddAsync(release);
                await context.Users.AddRangeAsync(existingUsers);
                await context.UserReleaseInvites.AddRangeAsync(existingReleaseInvites);
                await context.UserReleaseRoles.AddRangeAsync(existingRoles);
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
                    release.Id,
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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.InvitePreReleaseUsers(
                    release.Id,
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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseInvites.AddAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "invited.prerelease@test.com"
                    }
                );

                await context.UserReleaseRoles.AddAsync(
                    new UserReleaseRole
                    {
                        Release = release,
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
                    release.Id,
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication",
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved,
            };

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddAsync(user);

                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(release, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

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
                    release.Id,
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
                Assert.Equal(release.Id, savedUserReleaseRole!.ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRole.Role);
                Assert.Equal(user.Id, savedUserReleaseRole.UserId);

                Assert.Empty(context.UserReleaseInvites);
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_FailsSendingEmail_ExistingUser_ApprovedRelease()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved,
            };

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddAsync(user);

                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(release, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

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
                    release.Id,
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication",
                },
                ApprovalStatus = ReleaseApprovalStatus.Draft,
            };

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddAsync(user);

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
                    release.Id,
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
                Assert.Equal(release.Id, savedUserReleaseRole!.ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRole.Role);
                Assert.Equal(user.Id, savedUserReleaseRole.UserId);

                // Release invite is created for an existing user if the release is not approved
                var userReleaseInvite = Assert.Single(context.UserReleaseInvites);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(release.Id, userReleaseInvite.ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, userReleaseInvite.Role);
                Assert.False(userReleaseInvite.EmailSent); // Email not sent immediately for unapproved releases
            }
        }

        [Fact]
        public async Task InvitePreReleaseUsers_InvitesNewUser_ApprovedRelease()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication",
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>();

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(release, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

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
                    release.Id,
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
                Assert.Equal(release.Id, releaseInvite!.ReleaseId);
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>();

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(release, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

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
                    release.Id,
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication",
                },
                ApprovalStatus = ReleaseApprovalStatus.Draft,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
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
                    release.Id,
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
                Assert.Equal(release.Id, releaseInvite!.ReleaseId);
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer,
                    EmailSent = true
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    Release = release,
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
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Releases.AddAsync(release);
                await context.Users.AddRangeAsync(existingUsers);
                await context.UserReleaseInvites.AddRangeAsync(existingReleaseInvites);
                await context.UserReleaseRoles.AddRangeAsync(existingRoles);
                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValuesNewUser = GetExpectedPreReleaseTemplateValues(release, newUser: true);

            emailService.Setup(mock => mock.SendEmail(
                    It.IsIn("new.user.1@test.com", "new.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesNewUser
                ))
                .Returns(Unit.Instance);

            var expectedTemplateValuesExistingUser = GetExpectedPreReleaseTemplateValues(release, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    It.IsIn("existing.user.1@test.com", "existing.user.2@test.com"),
                    PreReleaseTemplateId,
                    expectedTemplateValuesExistingUser
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

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
                    release.Id,
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

                Assert.True(releaseInvites.All(invite => invite.ReleaseId == release.Id));
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

                Assert.True(roles.All(role => role.ReleaseId == release.Id));
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
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = PublishedScheduledStartOfDay,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var existingReleaseInvites = new List<UserReleaseInvite>
            {
                new()
                {
                    Email = "invited.prerelease.1@test.com",
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    Email = "invited.prerelease.2@test.com",
                    Release = release,
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
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                },
                new()
                {
                    User = existingUsers.Single(u => u.Email == "existing.prerelease.user.2@test.com"),
                    Release = release,
                    Role = ReleaseRole.PrereleaseViewer
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Releases.AddAsync(release);
                await context.Users.AddRangeAsync(existingUsers);
                await context.UserReleaseInvites.AddRangeAsync(existingReleaseInvites);
                await context.UserReleaseRoles.AddRangeAsync(existingRoles);
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
                    release.Id,
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

                Assert.True(releaseInvites.All(invite => invite.ReleaseId == release.Id));
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

                Assert.True(roles.All(role => role.ReleaseId == release.Id));
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
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.RemovePreReleaseUser(release.Id, "not an email");

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_ReleaseRoleRemoved()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddAsync(
                    new UserReleaseRole
                    {
                        Release = release,
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
                    release.Id,
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
            var release = new Release();
            var otherRelease = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Belongs to another release
                    new UserReleaseRole
                    {
                        Release = otherRelease,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Has a different role on the release
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
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
                    release.Id,
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

                Assert.Equal(otherRelease.Id, savedUserReleaseRoles[0].ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRoles[0].Role);

                Assert.Equal(release.Id, savedUserReleaseRoles[1].ReleaseId);
                Assert.Equal(ReleaseRole.Lead, savedUserReleaseRoles[1].Role);
            }
        }

        [Fact]
        public async Task RemovePreReleaseUser_AllInvitesRemoved()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
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
                    release.Id,
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
            var release = new Release();
            var otherRelease = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for another release
                    new UserReleaseInvite
                    {
                        Release = otherRelease,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for a different release role
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
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
                    release.Id,
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
                Assert.Equal(otherRelease.Id, savedUserReleaseInvites[0].ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseInvites[0].Role);
                Assert.Equal("test@test.com", savedUserReleaseInvites[0].Email);

                Assert.Equal(release.Id, savedUserReleaseInvites[1].ReleaseId);
                Assert.Equal(ReleaseRole.Lead, savedUserReleaseInvites[1].Role);
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
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
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
                    release.Id,
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

        [Fact]
        public async Task SendPreReleaseUserInviteEmails()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Publication = new Publication {Title = "Test publication"},
                PublishScheduled = PublishedScheduledStartOfDay
            };
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    release,
                    new User
                    {
                        Email = "test@test.com",
                    },
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com",
                        EmailSent = false,
                    },
                    new User
                    {
                        Email = "test2@test.com"
                    },
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test2@test.com",
                        EmailSent = true,
                    }
                );

                await context.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(release, newUser: false);

            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    PreReleaseTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, release);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.SendPreReleaseUserInviteEmails(release.Id);

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        PreReleaseTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(emailService, preReleaseService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var updatedInvite = context.UserReleaseInvites
                    .Single(i => i.Email == "test@test.com");

                Assert.Equal(release.Id, updatedInvite.ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, updatedInvite.Role);
                Assert.True(updatedInvite.EmailSent);
            }
        }

        private static Dictionary<string, dynamic> GetExpectedPreReleaseTemplateValues(Release release, bool newUser)
        {
            return new()
            {
                {"newUser", newUser ? "yes" : "no"},
                {"release name", "Calendar year 2020"},
                {"publication name", "Test publication"},
                {
                    "prerelease link",
                    $"http://localhost/publication/{release.PublicationId}/release/{release.Id}/prerelease/content"
                },
                {"prerelease day", "Tuesday 08 September 2020"},
                {"prerelease time", "09:30"},
                {"publish day", "Wednesday 09 September 2020"},
                {"publish time", "09:30"}
            };
        }

        private static void SetupGetPrereleaseWindow(Mock<IPreReleaseService> preReleaseService, Release release)
        {
            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(It.Is<Release>(r => r.Id == release.Id)))
                .Returns(
                    new PreReleaseWindow
                    {
                        Start = DateTime.Parse("2020-09-08T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                        End = DateTime.Parse("2020-09-08T22:59:59.00Z", styles: DateTimeStyles.AdjustToUniversal)
                    }
                );
        }

        private static Mock<IConfiguration> DefaultConfigurationMock()
        {
            return CreateMockConfiguration(
                TupleOf("NotifyPreReleaseTemplateId", PreReleaseTemplateId));
        }

        private static Mock<IHttpContextAccessor> DefaultHttpContextAccessorMock()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");

            httpContextAccessor
                .SetupGet(m => m.HttpContext)
                .Returns(context);

            return httpContextAccessor;
        }

        private static PreReleaseUserService SetupPreReleaseUserService(
            ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext,
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
                context,
                usersAndRolesDbContext,
                configuration ?? DefaultConfigurationMock().Object,
                emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
                preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                userRepository ?? new UserRepository(context),
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(context),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(context),
                httpContextAccessor ?? DefaultHttpContextAccessorMock().Object
            );
        }
    }
}
