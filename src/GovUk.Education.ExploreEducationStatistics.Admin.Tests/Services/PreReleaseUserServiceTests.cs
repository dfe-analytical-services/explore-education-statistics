#nullable enable
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseUserServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

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

        var emails = ListOf(
            "invited.prerelease@test.com",
            "existing.prerelease.user@test.com");

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                "invited.prerelease@test.com",
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(false)
            .Verifiable();
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                "existing.prerelease.user@test.com",
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(true)
            .Verifiable();

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvite(
                releaseVersion.Id,
                "invited.prerelease@test.com",
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(true)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object);

            var result = await service.GetPreReleaseUsersInvitePlan(
                releaseVersion.Id,
                emails
            );

            result.AssertBadRequest(NoInvitableEmails);
        }

        VerifyAllMocks(userReleaseRoleAndInviteManager, userReleaseInviteRepository);
    }

    [Fact]
    public async Task GetPreReleaseUsersInvitePlan()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var usersWithExistingReleaseInviteEmails = ListOf(
            "invited.prerelease.1@test.com",
            "invited.prerelease.2@test.com");

        var usersWithExistingRoleEmails = ListOf(
            "existing.prerelease.user.1@test.com",
            "existing.prerelease.user.2@test.com");

        var allEmails = ListOf(
            "new.user.1@test.com",
            "new.user.2@test.com",
            "existing.user.1@test.com",
            "existing.user.2@test.com")
            .Concat(usersWithExistingReleaseInviteEmails)
            .Concat(usersWithExistingRoleEmails)
            .ToList();

        var contentDbContextId = Guid.NewGuid().ToString();
        var userAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (usersWithExistingRoleEmails.Contains(email))
            {
                userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();
        }

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (usersWithExistingRoleEmails.Contains(email))
            {
                continue;
            }

            if (usersWithExistingReleaseInviteEmails.Contains(email))
            {
                userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
        );

            var result = await service.GetPreReleaseUsersInvitePlan(
                releaseVersion.Id,
                allEmails
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

        VerifyAllMocks(
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
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

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                "invited.prerelease@test.com",
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(false)
            .Verifiable();
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                "existing.prerelease.user@test.com",
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(true)
            .Verifiable();

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvite(
                releaseVersion.Id,
                "invited.prerelease@test.com",
                ReleaseRole.PrereleaseViewer
            ))
            .ReturnsAsync(true)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object);

            var result = await service.InvitePreReleaseUsers(
                releaseVersion.Id,
                ListOf(
                    "invited.prerelease@test.com",
                    "existing.prerelease.user@test.com")
            );

            result.AssertBadRequest(NoInvitableEmails);
        }

        VerifyAllMocks(
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
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

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Users.Add(user);
            await contentDbContext.SaveChangesAsync();
        }

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: false);

        emailService.Setup(mock => mock.SendEmail(
                user.Email,
                PreReleaseTemplateId,
                expectedTemplateValues
            ))
            .Returns(new BadRequestResult());

        var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
        SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                user.Email,
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer))
            .ReturnsAsync(false)
            .Verifiable();

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvite(
                releaseVersion.Id,
                user.Email,
                ReleaseRole.PrereleaseViewer))
            .ReturnsAsync(false)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                preReleaseService: preReleaseService.Object,
                emailService: emailService.Object,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
        );

            var result = await service.InvitePreReleaseUsers(
                releaseVersion.Id,
                ListOf(user.Email)
            );

            result.AssertLeft();
        }

        VerifyAllMocks(
            emailService,
            preReleaseService,
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
    }

    [Fact]
    public async Task InvitePreReleaseUsers_FailsSendingEmail_NewUser_ApprovedRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithPublishScheduled(PublishedScheduledStartOfDay);

        var email = "test@test.com";

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var emailService = new Mock<IEmailService>();

        var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: true);

        emailService.Setup(mock => mock.SendEmail(
                email,
                PreReleaseTemplateId,
                expectedTemplateValues
            ))
            .Returns(new BadRequestResult());

        var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
        SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.HasUserReleaseRole(
                email,
                releaseVersion.Id,
                ReleaseRole.PrereleaseViewer))
            .ReturnsAsync(false)
            .Verifiable();

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvite(
                releaseVersion.Id,
                email,
                ReleaseRole.PrereleaseViewer))
            .ReturnsAsync(false)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                preReleaseService: preReleaseService.Object,
                emailService: emailService.Object,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
            );

            var result = await service.InvitePreReleaseUsers(
                releaseVersion.Id,
                ListOf(email)
            );

            result.AssertLeft();
        }

        VerifyAllMocks(
            emailService,
            preReleaseService,
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
    }

    [Fact]
    public async Task InvitePreReleaseUsers_InvitesMultipleUsers_ApprovedRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithPublishScheduled(PublishedScheduledStartOfDay);

        var emailsWithExistingReleaseInvites = ListOf(
            "invited.prerelease.1@test.com",
            "invited.prerelease.2@test.com");

        var emailsWithExistingRoles = ListOf(
            "existing.prerelease.user.1@test.com",
            "existing.prerelease.user.2@test.com");

        var existingEmailsWithNoRolesOrInvites = ListOf(
            "existing.user.1@test.com",
            "existing.user.2@test.com");

        var allEmails = ListOf(
            "new.user.1@test.com",
            "new.user.2@test.com")
            .Concat(existingEmailsWithNoRolesOrInvites)
            .Concat(emailsWithExistingReleaseInvites)
            .Concat(emailsWithExistingRoles)
            .ToList();

        var existingUsersByEmail = existingEmailsWithNoRolesOrInvites
            .Concat(emailsWithExistingRoles)
            .Select(email => new User
            {
                Email = email
            })
            .ToDictionary(u => u.Email);

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Users.AddRange(existingUsersByEmail.Values);
            await contentDbContext.SaveChangesAsync();
        }

        var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
        SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        foreach (var email in allEmails)
        {
            if (emailsWithExistingRoles.Contains(email) || emailsWithExistingReleaseInvites.Contains(email))
            {
                continue;
            }

            var newUser = !existingUsersByEmail.ContainsKey(email);
            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: newUser);

            emailService.Setup(mock => mock.SendEmail(
                email,
                PreReleaseTemplateId,
                expectedTemplateValues
            ))
            .Returns(Unit.Instance)
            .Verifiable();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (emailsWithExistingRoles.Contains(email))
            {
                userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();

            if (existingUsersByEmail.ContainsKey(email))
            {
                userReleaseRoleAndInviteManager
                .Setup(mock => mock.CreateIfNotExists(
                    existingUsersByEmail[email].Id,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer,
                    _userId
                ))
                .ReturnsAsync(new UserReleaseRole())
                .Verifiable();
            }
        }

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (emailsWithExistingRoles.Contains(email))
            {
                continue;
            }

            if (emailsWithExistingReleaseInvites.Contains(email))
            {
                userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();

            if (!existingUsersByEmail.ContainsKey(email))
            {
                userReleaseInviteRepository
                    .Setup(mock => mock.Create(
                        releaseVersion.Id,
                        email,
                        ReleaseRole.PrereleaseViewer,
                        true,
                        _userId,
                        null))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            }
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailService: emailService.Object,
                preReleaseService: preReleaseService.Object,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
            );

            var result = await service.InvitePreReleaseUsers(
                releaseVersion.Id,
                allEmails);

            var preReleaseUsers = result.AssertRight();

            // The addresses that are already invited or accepted should have been ignored
            Assert.Equal(4, preReleaseUsers.Count);

            Assert.Equal("new.user.1@test.com", preReleaseUsers[0].Email);
            Assert.Equal("new.user.2@test.com", preReleaseUsers[1].Email);
            Assert.Equal("existing.user.1@test.com", preReleaseUsers[2].Email);
            Assert.Equal("existing.user.2@test.com", preReleaseUsers[3].Email);
        }

        VerifyAllMocks(
            emailService,
            preReleaseService,
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
    }

    [Fact]
    public async Task InvitePreReleaseUsers_InvitesMultipleUsers_DraftRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
            .WithPublishScheduled(PublishedScheduledStartOfDay);

        var emailsWithExistingReleaseInvites = ListOf(
            "invited.prerelease.1@test.com",
            "invited.prerelease.2@test.com");

        var emailsWithExistingRoles = ListOf(
            "existing.prerelease.user.1@test.com",
            "existing.prerelease.user.2@test.com");

        var existingEmailsWithNoRolesOrInvites = ListOf(
            "existing.user.1@test.com",
            "existing.user.2@test.com");

        var allEmails = ListOf(
            "new.user.1@test.com",
            "new.user.2@test.com")
            .Concat(existingEmailsWithNoRolesOrInvites)
            .Concat(emailsWithExistingReleaseInvites)
            .Concat(emailsWithExistingRoles)
            .ToList();

        var existingUsersByEmail = existingEmailsWithNoRolesOrInvites
            .Concat(emailsWithExistingRoles)
            .Select(email => new User
            {
                Email = email
            })
            .ToDictionary(u => u.Email);

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Users.AddRange(existingUsersByEmail.Values);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (emailsWithExistingRoles.Contains(email))
            {
                userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseRoleAndInviteManager
                .Setup(mock => mock.HasUserReleaseRole(
                    email,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();

            if (existingUsersByEmail.ContainsKey(email))
            {
                userReleaseRoleAndInviteManager
                .Setup(mock => mock.CreateIfNotExists(
                    existingUsersByEmail[email].Id,
                    releaseVersion.Id,
                    ReleaseRole.PrereleaseViewer,
                    _userId
                ))
                .ReturnsAsync(new UserReleaseRole())
                .Verifiable();
            }
        }

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
        foreach (var email in allEmails)
        {
            if (emailsWithExistingRoles.Contains(email))
            {
                continue;
            }

            if (emailsWithExistingReleaseInvites.Contains(email))
            {
                userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(true)
                .Verifiable();

                continue;
            }

            userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(
                    releaseVersion.Id,
                    email,
                    ReleaseRole.PrereleaseViewer
                ))
                .ReturnsAsync(false)
                .Verifiable();

            userReleaseInviteRepository
            .Setup(mock => mock.Create(
                releaseVersion.Id,
                email,
                ReleaseRole.PrereleaseViewer,
                false,
                _userId,
                null))
            .Returns(Task.CompletedTask)
            .Verifiable();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object
            );

            var result = await service.InvitePreReleaseUsers(
                releaseVersion.Id,
                allEmails);

            var preReleaseUsers = result.AssertRight();

            // The addresses that are already invited or accepted should have been ignored
            Assert.Equal(4, preReleaseUsers.Count);

            Assert.Equal("new.user.1@test.com", preReleaseUsers[0].Email);
            Assert.Equal("new.user.2@test.com", preReleaseUsers[1].Email);
            Assert.Equal("existing.user.1@test.com", preReleaseUsers[2].Email);
            Assert.Equal("existing.user.2@test.com", preReleaseUsers[3].Email);
        }

        VerifyAllMocks(
            userReleaseRoleAndInviteManager,
            userReleaseInviteRepository);
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
    public async Task RemovePreReleaseUser_UserDoesNotExist()
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
            var service = SetupPreReleaseUserService(
                contentDbContext: context, 
                usersAndRolesDbContext: userAndRolesDbContext);

            var result = await service.RemovePreReleaseUser(releaseVersion.Id, "test@test.com");

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemovePreReleaseUser_NoRemainingReleaseInvites_AcceptedInvite()
    {
        var user = new User
        {
            Email = "test@test.com"
        };

        var unacceptedUserInvite = new UserInvite
        {
            Email = user.Email,
            RoleId = Role.PrereleaseUser.GetEnumValue(),
            Accepted = true,
            Created = DateTime.UtcNow
        };

        var releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.UserInvites.Add(unacceptedUserInvite);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(m => m.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                releaseVersion.Id,
                user.Id,
                default,
                ReleaseRole.PrereleaseViewer
            ))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object
            );

            var result = await service.RemovePreReleaseUser(
                releaseVersion.Id,
                user.Email
            );

            result.AssertRight();
        }

        VerifyAllMocks(userReleaseRoleAndInviteManager);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            // The UserInvite should not have been removed
            var remainingUserInvites = await usersAndRolesDbContext.UserInvites
            .ToListAsync();

            var remainingUserInvite = Assert.Single(remainingUserInvites);

            Assert.Equal(user.Email, remainingUserInvite.Email);
            Assert.True(remainingUserInvite.Accepted);
            Assert.Equal(Role.PrereleaseUser.GetEnumValue(), remainingUserInvite.RoleId);
        }
    }

    [Fact]
    public async Task RemovePreReleaseUser_RemainingReleaseInvites()
    {
        var user = new User
        {
            Email = "test@test.com"
        };

        var unacceptedUserInvite = new UserInvite
        {
            Email = user.Email,
            RoleId = Role.PrereleaseUser.GetEnumValue(),
            Accepted = false,
            Created = DateTime.UtcNow
        };

        var releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .Generate();

        var userReleaseInvite = _dataFixture.DefaultUserReleaseInvite()
            .WithReleaseVersion(releaseVersion)
            .WithEmail(user.Email)
            .WithRole(ReleaseRole.PrereleaseViewer)
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.UserReleaseInvites.Add(userReleaseInvite);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.UserInvites.Add(unacceptedUserInvite);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(m => m.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                releaseVersion.Id,
                user.Id,
                default,
                ReleaseRole.PrereleaseViewer
            ))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object
            );

            var result = await service.RemovePreReleaseUser(
                releaseVersion.Id,
                user.Email
            );

            result.AssertRight();
        }

        VerifyAllMocks(userReleaseRoleAndInviteManager);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            // The UserInvite should not have been removed
            var remainingUserInvites = await usersAndRolesDbContext.UserInvites
            .ToListAsync();

            var remainingUserInvite = Assert.Single(remainingUserInvites);

            Assert.Equal(user.Email, remainingUserInvite.Email);
            Assert.False(remainingUserInvite.Accepted);
            Assert.Equal(Role.PrereleaseUser.GetEnumValue(), remainingUserInvite.RoleId);
        }
    }

    [Fact]
    public async Task RemovePreReleaseUser_NoRemainingReleaseInvites()
    {
        var user = new User
        {
            Email = "test@test.com"
        };

        var unacceptedUserInvite = new UserInvite
        {
            Email = user.Email,
            RoleId = Role.PrereleaseUser.GetEnumValue(),
            Accepted = false
        };

        var releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        var usersAndRolesDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            usersAndRolesDbContext.UserInvites.Add(unacceptedUserInvite);
            await usersAndRolesDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict);
        userReleaseRoleAndInviteManager
            .Setup(m => m.RemoveAllRolesAndInvitesForReleaseVersionAndUser(
                releaseVersion.Id,
                user.Id,
                default,
                ReleaseRole.PrereleaseViewer
            ))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupPreReleaseUserService(
                contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object
            );

            var result = await service.RemovePreReleaseUser(
                releaseVersion.Id,
                user.Email
            );

            result.AssertRight();
        }

        VerifyAllMocks(userReleaseRoleAndInviteManager);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            // The unaccepted UserInvite should have been removed
            var remainingUserInvites = await usersAndRolesDbContext.UserInvites
            .ToListAsync();

            Assert.Empty(remainingUserInvites);
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
        ContentDbContext contentDbContext,
        UsersAndRolesDbContext usersAndRolesDbContext,
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
        userRepository ??= new UserRepository(contentDbContext);
        userReleaseInviteRepository ??= new UserReleaseInviteRepository(
            contentDbContext: contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseInviteRepository>>());

        return new(
            contentDbContext,
            usersAndRolesDbContext,
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions(),
            preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService(_userId).Object,
            userRepository,
            userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
            userReleaseRoleAndInviteManager ?? Mock.Of<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(MockBehavior.Strict)
        );
    }
}
