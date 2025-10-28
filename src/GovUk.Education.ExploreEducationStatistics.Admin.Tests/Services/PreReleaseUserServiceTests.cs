#nullable enable
using System.Globalization;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class PreReleaseUserServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid _userId = Guid.NewGuid();

    private const string PreReleaseTemplateId = "prerelease-template-id";

    private static readonly DateTime PublishedScheduledStartOfDay = new DateTime(
        2020,
        09,
        09
    ).AsStartOfDayUtcForTimeZone();

    public class GetPreReleaseUsersTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Add roles for existing users
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = _dataFixture.DefaultUser().WithEmail("existing.1@test.com"),
                    },
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = _dataFixture.DefaultUser().WithEmail("existing.2@test.com"),
                    }
                );

                await context.AddRangeAsync(
                    // Add invites for new users
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    },
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    },
                    // Existing users may also have invites depending on their state and the release status at the time of being invited
                    // * If they were a new user then an invite will exist
                    // * If the release was draft an invite will exist (since emails are sent on approval based on invites)
                    new UserReleaseInvite
                    {
                        ReleaseVersion = releaseVersion,
                        Email = "existing.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context);
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
        public async Task OrderedCorrectly()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = _dataFixture.DefaultUser().WithEmail("existing.2@test.com"),
                    },
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = _dataFixture.DefaultUser().WithEmail("existing.1@test.com"),
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
            {
                var service = SetupPreReleaseUserService(context);
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
        public async Task FiltersInvalidReleaseUsers()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseRole
                    {
                        ReleaseVersion = releaseVersion,
                        Role = ReleaseRole.Contributor,
                        User = _dataFixture.DefaultUser().WithEmail("existing.1@test.com"),
                    },
                    // Different release user
                    new UserReleaseRole
                    {
                        ReleaseVersion = otherReleaseVersion,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = _dataFixture.DefaultUser().WithEmail("existing.2@test.com"),
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }

        [Fact]
        public async Task FiltersInvalidReleaseInvites()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            ReleaseVersion otherReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

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
            {
                var service = SetupPreReleaseUserService(context);
                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();
                Assert.Empty(users);
            }
        }
    }

    public class GetPreReleaseUsersInvitePlanTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task InvalidEmail_ReturnsBadRequest()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context);
                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    ListOf("test1@test.com", "not an email", "test2@test.com")
                );

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task NoRelease_ReturnsNotFound()
        {
            await using var context = InMemoryApplicationDbContext();
            var service = SetupPreReleaseUserService(context);
            var result = await service.GetPreReleaseUsersInvitePlan(Guid.NewGuid(), ListOf("test@test.com"));

            result.AssertNotFound();
        }

        [Fact]
        public async Task NoInvitableEmails_ReturnsBadRequest()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var emails = ListOf("invited.prerelease@test.com", "existing.prerelease.user@test.com");

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.HasUserReleaseRole(
                        "invited.prerelease@test.com",
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer
                    )
                )
                .ReturnsAsync(false);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.HasUserReleaseRole(
                        "existing.prerelease.user@test.com",
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer
                    )
                )
                .ReturnsAsync(true);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(mock =>
                    mock.UserHasInvite(releaseVersion.Id, "invited.prerelease@test.com", ReleaseRole.PrereleaseViewer)
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, emails);

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userReleaseRoleRepository, userReleaseInviteRepository);
        }

        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var usersWithExistingReleaseInviteEmails = ListOf(
                "invited.prerelease.1@test.com",
                "invited.prerelease.2@test.com"
            );

            var usersWithExistingRoleEmails = ListOf(
                "existing.prerelease.user.1@test.com",
                "existing.prerelease.user.2@test.com"
            );

            var allEmails = ListOf(
                    "new.user.1@test.com",
                    "new.user.2@test.com",
                    "existing.user.1@test.com",
                    "existing.user.2@test.com"
                )
                .Concat(usersWithExistingReleaseInviteEmails)
                .Concat(usersWithExistingRoleEmails)
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (usersWithExistingRoleEmails.Contains(email))
                {
                    userReleaseRoleRepository
                        .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseRoleRepository
                    .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);
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
                        .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseInviteRepository
                    .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, allEmails);

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

            VerifyAllMocks(userReleaseRoleRepository, userReleaseInviteRepository);
        }
    }

    public class InvitePreReleaseUsersTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task InvalidEmail_ReturnsBadRequest()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context);
                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("test1@test.com", "not an email", "test2@test.com")
                );

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task NoRelease_ReturnsNotFound()
        {
            await using var context = InMemoryApplicationDbContext();
            var service = SetupPreReleaseUserService(context);
            var result = await service.InvitePreReleaseUsers(Guid.NewGuid(), ListOf("test@test.com"));

            result.AssertNotFound();
        }

        [Fact]
        public async Task NoInvitableEmails_ReturnsBadRequest()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.HasUserReleaseRole(
                        "invited.prerelease@test.com",
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer
                    )
                )
                .ReturnsAsync(false);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.HasUserReleaseRole(
                        "existing.prerelease.user@test.com",
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer
                    )
                )
                .ReturnsAsync(true);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(mock =>
                    mock.UserHasInvite(releaseVersion.Id, "invited.prerelease@test.com", ReleaseRole.PrereleaseViewer)
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf("invited.prerelease@test.com", "existing.prerelease.user@test.com")
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userReleaseRoleRepository, userReleaseInviteRepository);
        }

        [Fact]
        public async Task ActiveUserForApprovedReleaseFailsSendingEmail_Fails()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: false);

            emailService
                .Setup(mock => mock.SendEmail(user.Email, PreReleaseTemplateId, expectedTemplateValues))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock => mock.HasUserReleaseRole(user.Email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                .ReturnsAsync(false);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(releaseVersion.Id, user.Email, ReleaseRole.PrereleaseViewer))
                .ReturnsAsync(false);

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(releaseVersion.Id, ListOf(user.Email));

                result.AssertLeft();
            }

            VerifyAllMocks(
                emailService,
                preReleaseService,
                userReleaseRoleRepository,
                userReleaseInviteRepository,
                userRepository
            );
        }

        [Fact]
        public async Task NewUserForApprovedReleaseFailsSendingEmail_Fails()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            User userToCreate = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>();

            var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(releaseVersion, newUser: true);

            emailService
                .Setup(mock => mock.SendEmail(userToCreate.Email, PreReleaseTemplateId, expectedTemplateValues))
                .Returns(new BadRequestResult());

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.HasUserReleaseRole(userToCreate.Email, releaseVersion.Id, ReleaseRole.PrereleaseViewer)
                )
                .ReturnsAsync(false);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.UserHasInvite(releaseVersion.Id, userToCreate.Email, ReleaseRole.PrereleaseViewer))
                .ReturnsAsync(false);

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(userToCreate.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        userToCreate.Email,
                        GlobalRoles.Role.PrereleaseUser,
                        _userId,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(releaseVersion.Id, ListOf(userToCreate.Email));

                result.AssertLeft();
            }

            VerifyAllMocks(
                emailService,
                preReleaseService,
                userReleaseRoleRepository,
                userReleaseInviteRepository,
                userRepository
            );
        }

        [Fact]
        public async Task InvitesMultipleUsersForApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var usersWithExistingReleaseInvitesByEmail = _dataFixture
                .DefaultUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var usersWithExistingRolesByEmail = _dataFixture.DefaultUser().GenerateList(2).ToDictionary(u => u.Email);

            var activeUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var pendingInviteUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUserWithPendingInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var softDeletedUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultSoftDeletedUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var expiredInviteUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUserWithExpiredInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var newUserEmails = ListOf("new.user.1@test.com", "new.user.2@test.com");

            var existingUsersByEmail = usersWithExistingReleaseInvitesByEmail
                .Concat(usersWithExistingRolesByEmail)
                .Concat(activeUsersWithNoRolesOrInvitesByEmail)
                .Concat(pendingInviteUsersWithNoRolesOrInvitesByEmail)
                .Concat(softDeletedUsersWithNoRolesOrInvitesByEmail)
                .Concat(expiredInviteUsersWithNoRolesOrInvitesByEmail)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var allEmails = existingUsersByEmail.Values.Select(u => u.Email).Concat(newUserEmails).ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            SetupGetPrereleaseWindow(preReleaseService, releaseVersion);

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingReleaseInvitesByEmail.ContainsKey(email)
                    || usersWithExistingRolesByEmail.ContainsKey(email)
                )
                {
                    continue;
                }

                var classedAsNewUserForEmail = !activeUsersWithNoRolesOrInvitesByEmail.ContainsKey(email);
                var expectedTemplateValues = GetExpectedPreReleaseTemplateValues(
                    releaseVersion,
                    classedAsNewUserForEmail
                );

                emailService
                    .Setup(mock => mock.SendEmail(email, PreReleaseTemplateId, expectedTemplateValues))
                    .Returns(Unit.Instance);
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (usersWithExistingRolesByEmail.ContainsKey(email))
                {
                    userReleaseRoleRepository
                        .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseRoleRepository
                    .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);

                if (activeUsersWithNoRolesOrInvitesByEmail.TryGetValue(email, out var activeUser))
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.CreateIfNotExists(
                                activeUser.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                _userId
                            )
                        )
                        .ReturnsAsync(new UserReleaseRole());
                }
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (usersWithExistingRolesByEmail.ContainsKey(email))
                {
                    continue;
                }

                if (usersWithExistingReleaseInvitesByEmail.ContainsKey(email))
                {
                    userReleaseInviteRepository
                        .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseInviteRepository
                    .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);

                if (!activeUsersWithNoRolesOrInvitesByEmail.ContainsKey(email))
                {
                    userReleaseInviteRepository
                        .Setup(mock =>
                            mock.Create(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer, true, _userId, null)
                        )
                        .Returns(Task.CompletedTask);
                }
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRolesByEmail.ContainsKey(email)
                    || usersWithExistingReleaseInvitesByEmail.ContainsKey(email)
                )
                {
                    continue;
                }

                if (activeUsersWithNoRolesOrInvitesByEmail.TryGetValue(email, out var activeUser))
                {
                    userRepository
                        .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(activeUser);

                    continue;
                }

                userRepository
                    .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

                userRepository
                    .Setup(mock =>
                        mock.CreateOrUpdate(
                            email,
                            GlobalRoles.Role.PrereleaseUser,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(_dataFixture.DefaultUser());
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    emailService: emailService.Object,
                    preReleaseService: preReleaseService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(releaseVersion.Id, allEmails);

                var preReleaseUsers = result.AssertRight();

                // The users that already have the release role or invite should be filtered out
                Assert.Equal(10, preReleaseUsers.Count);

                var expectedEmails = new[]
                {
                    activeUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    activeUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    pendingInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    pendingInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    softDeletedUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    softDeletedUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    expiredInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    expiredInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    newUserEmails[0],
                    newUserEmails[1],
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            VerifyAllMocks(
                emailService,
                preReleaseService,
                userReleaseRoleRepository,
                userReleaseInviteRepository,
                userRepository
            );
        }

        [Fact]
        public async Task InvitesMultipleUsersForDraftRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var usersWithExistingReleaseInvitesByEmail = _dataFixture
                .DefaultUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var usersWithExistingRolesByEmail = _dataFixture.DefaultUser().GenerateList(2).ToDictionary(u => u.Email);

            var activeUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var pendingInviteUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUserWithPendingInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var softDeletedUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultSoftDeletedUser()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var expiredInviteUsersWithNoRolesOrInvitesByEmail = _dataFixture
                .DefaultUserWithExpiredInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var newUserEmails = ListOf("new.user.1@test.com", "new.user.2@test.com");

            var existingUsersByEmail = usersWithExistingReleaseInvitesByEmail
                .Concat(usersWithExistingRolesByEmail)
                .Concat(activeUsersWithNoRolesOrInvitesByEmail)
                .Concat(pendingInviteUsersWithNoRolesOrInvitesByEmail)
                .Concat(softDeletedUsersWithNoRolesOrInvitesByEmail)
                .Concat(expiredInviteUsersWithNoRolesOrInvitesByEmail)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var allEmails = existingUsersByEmail.Values.Select(u => u.Email).Concat(newUserEmails).ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (usersWithExistingRolesByEmail.ContainsKey(email))
                {
                    userReleaseRoleRepository
                        .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseRoleRepository
                    .Setup(mock => mock.HasUserReleaseRole(email, releaseVersion.Id, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);

                if (activeUsersWithNoRolesOrInvitesByEmail.TryGetValue(email, out var activeUser))
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.CreateIfNotExists(
                                activeUser.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                _userId
                            )
                        )
                        .ReturnsAsync(new UserReleaseRole());
                }
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (usersWithExistingRolesByEmail.ContainsKey(email))
                {
                    continue;
                }

                if (usersWithExistingReleaseInvitesByEmail.ContainsKey(email))
                {
                    userReleaseInviteRepository
                        .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                        .ReturnsAsync(true);

                    continue;
                }

                userReleaseInviteRepository
                    .Setup(mock => mock.UserHasInvite(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer))
                    .ReturnsAsync(false);

                userReleaseInviteRepository
                    .Setup(mock =>
                        mock.Create(releaseVersion.Id, email, ReleaseRole.PrereleaseViewer, false, _userId, null)
                    )
                    .Returns(Task.CompletedTask);
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRolesByEmail.ContainsKey(email)
                    || usersWithExistingReleaseInvitesByEmail.ContainsKey(email)
                )
                {
                    continue;
                }

                if (activeUsersWithNoRolesOrInvitesByEmail.TryGetValue(email, out var activeUser))
                {
                    userRepository
                        .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(activeUser);

                    continue;
                }

                userRepository
                    .Setup(mock => mock.FindActiveUserByEmail(email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

                userRepository
                    .Setup(mock =>
                        mock.CreateOrUpdate(
                            email,
                            GlobalRoles.Role.PrereleaseUser,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(_dataFixture.DefaultUser());
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(releaseVersion.Id, allEmails);

                var preReleaseUsers = result.AssertRight();

                // The users that already have the release role or invite should be filtered out
                Assert.Equal(10, preReleaseUsers.Count);

                var expectedEmails = new[]
                {
                    activeUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    activeUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    pendingInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    pendingInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    softDeletedUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    softDeletedUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    expiredInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(0).Value.Email,
                    expiredInviteUsersWithNoRolesOrInvitesByEmail.ElementAt(1).Value.Email,
                    newUserEmails[0],
                    newUserEmails[1],
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            VerifyAllMocks(userReleaseRoleRepository, userReleaseInviteRepository, userRepository);
        }

        private static Dictionary<string, dynamic> GetExpectedPreReleaseTemplateValues(
            ReleaseVersion releaseVersion,
            bool newUser
        )
        {
            return new()
            {
                { "newUser", newUser ? "yes" : "no" },
                { "release name", releaseVersion.Release.Title },
                { "publication name", releaseVersion.Release.Publication.Title },
                {
                    "prerelease link",
                    $"http://localhost/publication/{releaseVersion.Release.PublicationId}/release/{releaseVersion.Id}/prerelease/content"
                },
                { "prerelease day", "Tuesday 08 September 2020" },
                { "prerelease time", "09:30" },
                { "publish day", "Wednesday 09 September 2020" },
                { "publish time", "09:30" },
            };
        }

        private static void SetupGetPrereleaseWindow(
            Mock<IPreReleaseService> preReleaseService,
            ReleaseVersion releaseVersion
        )
        {
            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
                .Returns(
                    new PreReleaseWindow
                    {
                        Start = DateTime.Parse("2020-09-08T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                        ScheduledPublishDate = DateTime.Parse(
                            "2020-09-09T00:00:00.00Z",
                            styles: DateTimeStyles.AdjustToUniversal
                        ),
                    }
                );
        }
    }

    public class RemovePreReleaseUserTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task InvalidEmail_ReturnsBadRequest()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(context);
                var result = await service.RemovePreReleaseUser(releaseVersion.Id, "not an email");

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNotFound()
        {
            var email = "test@test.com";

            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: context,
                    userRepository: userRepository.Object
                );

                var result = await service.RemovePreReleaseUser(releaseVersion.Id, email);

                result.AssertNotFound();
            }

            VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();

            var releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .Generate();

            var userReleaseInvite = _dataFixture
                .DefaultUserReleaseInvite()
                .WithReleaseVersion(releaseVersion)
                .WithEmail(user.Email)
                .WithRole(ReleaseRole.PrereleaseViewer)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(m =>
                    m.RemoveForReleaseVersionAndUser(releaseVersion.Id, user.Id, default, ReleaseRole.PrereleaseViewer)
                )
                .Returns(Task.CompletedTask);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(MockBehavior.Strict);
            userReleaseInviteRepository
                .Setup(m =>
                    m.RemoveByReleaseVersionAndEmail(
                        releaseVersion.Id,
                        user.Email,
                        default,
                        ReleaseRole.PrereleaseViewer
                    )
                )
                .Returns(Task.CompletedTask)
                .Callback(async () =>
                {
                    await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);

                    var releaseInvite = await contentDbContext.UserReleaseInvites.SingleAsync(uri =>
                        uri.Id == userReleaseInvite.Id
                    );

                    contentDbContext.Remove(releaseInvite);
                    await contentDbContext.SaveChangesAsync();
                });

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemovePreReleaseUser(releaseVersion.Id, user.Email);

                result.AssertRight();
            }

            VerifyAllMocks(userReleaseRoleRepository, userReleaseInviteRepository, userRepository);
        }
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
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null,
        IPreReleaseService? preReleaseService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null
    )
    {
        userReleaseInviteRepository ??= new UserReleaseInviteRepository(
            contentDbContext: contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseInviteRepository>>()
        );

        return new(
            contentDbContext,
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions(),
            preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService(_userId).Object,
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(MockBehavior.Strict)
        );
    }
}
