#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
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

    private static readonly DateTimeOffset PublishedScheduledStartOfDay = new DateOnly(2020, 9, 9).GetUkStartOfDayUtc();

    public class GetPreReleaseUsersTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var userReleaseRoles = _dataFixture
                // These should be returned, regardless of whether or not the user
                // is active, pending or expired
                .DefaultUserReleaseRole()
                .ForIndex(
                    0,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion)
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetUser(_dataFixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(releaseVersion)
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetUser(_dataFixture.DefaultUserWithExpiredInvite())
                            .SetReleaseVersion(releaseVersion)
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                // This should be filtered out as it is for a different release version
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetReleaseVersion(
                                _dataFixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                    )
                            )
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                // These should be filtered out as they are not prerelease viewers
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion)
                            .SetRole(ReleaseRole.Contributor)
                )
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(_dataFixture.DefaultUser())
                            .SetReleaseVersion(releaseVersion)
                            .SetRole(ReleaseRole.Approver)
                )
                .GenerateList(6);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
            userReleaseRoleRepository.Setup(m => m.Query(ResourceRoleFilter.All)).Returns(userReleaseRoles.BuildMock());

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: context,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();

                var expectedRoleEmailsOrderedByEmail = userReleaseRoles
                    .Where(urr => urr.ReleaseVersion.Id == releaseVersion.Id)
                    .Where(urr => urr.Role == ReleaseRole.PrereleaseViewer)
                    .Select(urr => urr.User.Email)
                    .Order()
                    .ToList();

                var resultEmails = users.Select(u => u.Email).ToList();

                Assert.Equal(expectedRoleEmailsOrderedByEmail, resultEmails);
            }

            VerifyAllMocks(userReleaseRoleRepository);
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

            User activeUser = _dataFixture.DefaultUser();
            User userWithPendingInvite = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(activeUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);
            userRepository
                .Setup(mock => mock.FindUserByEmail(userWithPendingInvite.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userWithPendingInvite);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnReleaseVersion(
                        activeUser.Id,
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnReleaseVersion(
                        userWithPendingInvite.Id,
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    [activeUser.Email, userWithPendingInvite.Email]
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var usersWithPendingInviteAndPrereleaseRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndNoRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var activeUsersWithPrereleaseRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithNoRole = _dataFixture.DefaultUser().GenerateList(2);

            var usersWithPendingInviteAndPrereleaseRoleByEmail = usersWithPendingInviteAndPrereleaseRole.ToDictionary(
                u => u.Email
            );
            var usersWithPendingInviteAndNoRoleEmail = usersWithPendingInviteAndNoRole.ToDictionary(u => u.Email);
            var activeUsersWithPrereleaseRoleByEmail = activeUsersWithPrereleaseRole.ToDictionary(u => u.Email);
            var activeUsersWithNoRoleEmail = activeUsersWithNoRole.ToDictionary(u => u.Email);

            var allExistingUsersByEmail = usersWithPendingInviteAndPrereleaseRole
                .Concat(usersWithPendingInviteAndNoRole)
                .Concat(activeUsersWithPrereleaseRole)
                .Concat(activeUsersWithNoRole)
                .ToDictionary(u => u.Email);

            var allEmails = ListOf("new.user.1@test.com", "new.user.2@test.com")
                .Concat(allExistingUsersByEmail.Keys)
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (allExistingUsersByEmail.TryGetValue(email, out var user))
                {
                    userRepository
                        .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(user);

                    continue;
                }

                userRepository
                    .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (!allExistingUsersByEmail.ContainsKey(email))
                {
                    continue;
                }

                if (
                    usersWithPendingInviteAndPrereleaseRoleByEmail.TryGetValue(email, out var user)
                    || activeUsersWithPrereleaseRoleByEmail.TryGetValue(email, out user)
                )
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(true);

                    continue;
                }

                var userWithoutRole = usersWithPendingInviteAndNoRoleEmail.TryGetValue(
                    email,
                    out var userWithPendingInvite
                )
                    ? userWithPendingInvite
                    : activeUsersWithNoRoleEmail[email];

                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasRoleOnReleaseVersion(
                            userWithoutRole.Id,
                            releaseVersion.Id,
                            ReleaseRole.PrereleaseViewer,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userRepository: userRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, allEmails);

                var plan = result.AssertRight();

                Assert.Equal(2, plan.AlreadyAccepted.Count);
                Assert.Equal(activeUsersWithPrereleaseRole[0].Email, plan.AlreadyAccepted[0]);
                Assert.Equal(activeUsersWithPrereleaseRole[1].Email, plan.AlreadyAccepted[1]);

                Assert.Equal(2, plan.AlreadyInvited.Count);
                Assert.Equal(usersWithPendingInviteAndPrereleaseRole[0].Email, plan.AlreadyInvited[0]);
                Assert.Equal(usersWithPendingInviteAndPrereleaseRole[1].Email, plan.AlreadyInvited[1]);

                Assert.Equal(6, plan.Invitable.Count);
                Assert.Equal("new.user.1@test.com", plan.Invitable[0]);
                Assert.Equal("new.user.2@test.com", plan.Invitable[1]);
                Assert.Equal(usersWithPendingInviteAndNoRole[0].Email, plan.Invitable[2]);
                Assert.Equal(usersWithPendingInviteAndNoRole[1].Email, plan.Invitable[3]);
                Assert.Equal(activeUsersWithNoRole[0].Email, plan.Invitable[4]);
                Assert.Equal(activeUsersWithNoRole[1].Email, plan.Invitable[5]);
            }

            VerifyAllMocks(userRepository, userReleaseRoleRepository);
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

            User activeUser = _dataFixture.DefaultUser();
            User userWithPendingInvite = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindUserByEmail(activeUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);
            userRepository
                .Setup(mock => mock.FindUserByEmail(userWithPendingInvite.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userWithPendingInvite);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnReleaseVersion(
                        activeUser.Id,
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasRoleOnReleaseVersion(
                        userWithPendingInvite.Id,
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    ListOf([activeUser.Email, userWithPendingInvite.Email])
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userRepository, userReleaseRoleRepository);
        }

        [Fact]
        public async Task InvitesMultipleUsersForApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var usersWithPendingInvitesAndExistingRoleByEmail = _dataFixture
                .DefaultUserWithPendingInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var usersWithExistingRoleByEmail = _dataFixture.DefaultUser().GenerateList(2).ToDictionary(u => u.Email);

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

            var newUserIdsByEmail = ListOf<(Guid Id, string Email)>(
                    (Guid.NewGuid(), "new.user.1@test.com"),
                    (Guid.NewGuid(), "new.user.2@test.com")
                )
                .ToDictionary(t => t.Email, t => t.Id);

            var existingUsersByEmail = usersWithPendingInvitesAndExistingRoleByEmail
                .Concat(usersWithExistingRoleByEmail)
                .Concat(activeUsersWithNoRolesOrInvitesByEmail)
                .Concat(pendingInviteUsersWithNoRolesOrInvitesByEmail)
                .Concat(softDeletedUsersWithNoRolesOrInvitesByEmail)
                .Concat(expiredInviteUsersWithNoRolesOrInvitesByEmail)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var allEmails = existingUsersByEmail.Values.Select(u => u.Email).Concat(newUserIdsByEmail.Keys).ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userRepository
                        .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existingUser);
                }
                else
                {
                    userRepository
                        .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((User?)null);
                }

                if (
                    usersWithExistingRoleByEmail.ContainsKey(email)
                    || usersWithPendingInvitesAndExistingRoleByEmail.ContainsKey(email)
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
                    .ReturnsAsync(
                        _dataFixture
                            .DefaultUserWithPendingInvite()
                            .WithId(
                                existingUsersByEmail.TryGetValue(email, out var user)
                                    ? user.Id
                                    : newUserIdsByEmail[email]
                            )
                            .WithEmail(email)
                            .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
                            .WithCreatedById(_userId)
                            .WithCreated(DateTimeOffset.UtcNow)
                    );
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRoleByEmail.TryGetValue(email, out var user)
                    || usersWithPendingInvitesAndExistingRoleByEmail.TryGetValue(email, out user)
                )
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(true);

                    continue;
                }

                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasRoleOnReleaseVersion(
                                existingUser.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var userId = existingUser?.Id ?? newUserIdsByEmail[email];
                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.Create(
                            userId,
                            releaseVersion.Id,
                            ReleaseRole.PrereleaseViewer,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(new UserReleaseRole());
            }

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRoleByEmail.ContainsKey(email)
                    || usersWithPendingInvitesAndExistingRoleByEmail.ContainsKey(email)
                )
                {
                    continue;
                }

                var userId = existingUsersByEmail.TryGetValue(email, out var existingUser)
                    ? existingUser.Id
                    : newUserIdsByEmail[email];

                userResourceRoleNotificationService
                    .Setup(mock =>
                        mock.NotifyUserOfNewPreReleaseRole(userId, releaseVersion.Id, It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
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
                    newUserIdsByEmail.ElementAt(0).Key,
                    newUserIdsByEmail.ElementAt(1).Key,
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            VerifyAllMocks(userResourceRoleNotificationService, userReleaseRoleRepository, userRepository);
        }

        [Fact]
        public async Task InvitesMultipleUsersForDraftRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var usersWithPendingInvitesAndExistingRoleByEmail = _dataFixture
                .DefaultUserWithPendingInvite()
                .GenerateList(2)
                .ToDictionary(u => u.Email);

            var usersWithExistingRoleByEmail = _dataFixture.DefaultUser().GenerateList(2).ToDictionary(u => u.Email);

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

            var newUserIdsByEmail = ListOf<(Guid Id, string Email)>(
                    (Guid.NewGuid(), "new.user.1@test.com"),
                    (Guid.NewGuid(), "new.user.2@test.com")
                )
                .ToDictionary(t => t.Email, t => t.Id);

            var existingUsersByEmail = usersWithPendingInvitesAndExistingRoleByEmail
                .Concat(usersWithExistingRoleByEmail)
                .Concat(activeUsersWithNoRolesOrInvitesByEmail)
                .Concat(pendingInviteUsersWithNoRolesOrInvitesByEmail)
                .Concat(softDeletedUsersWithNoRolesOrInvitesByEmail)
                .Concat(expiredInviteUsersWithNoRolesOrInvitesByEmail)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var allEmails = existingUsersByEmail.Values.Select(u => u.Email).Concat(newUserIdsByEmail.Keys).ToList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userRepository
                        .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existingUser);
                }
                else
                {
                    userRepository
                        .Setup(mock => mock.FindUserByEmail(email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((User?)null);
                }

                if (
                    usersWithExistingRoleByEmail.ContainsKey(email)
                    || usersWithPendingInvitesAndExistingRoleByEmail.ContainsKey(email)
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
                    .ReturnsAsync(
                        _dataFixture
                            .DefaultUserWithPendingInvite()
                            .WithId(
                                existingUsersByEmail.TryGetValue(email, out var user)
                                    ? user.Id
                                    : newUserIdsByEmail[email]
                            )
                            .WithEmail(email)
                            .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
                            .WithCreatedById(_userId)
                            .WithCreated(DateTimeOffset.UtcNow)
                    );
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRoleByEmail.TryGetValue(email, out var user)
                    || usersWithPendingInvitesAndExistingRoleByEmail.TryGetValue(email, out user)
                )
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(true);

                    continue;
                }

                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userReleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasRoleOnReleaseVersion(
                                existingUser.Id,
                                releaseVersion.Id,
                                ReleaseRole.PrereleaseViewer,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var userId = existingUser?.Id ?? newUserIdsByEmail[email];
                userReleaseRoleRepository
                    .Setup(mock =>
                        mock.Create(
                            userId,
                            releaseVersion.Id,
                            ReleaseRole.PrereleaseViewer,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(new UserReleaseRole());
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
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
                    newUserIdsByEmail.ElementAt(0).Key,
                    newUserIdsByEmail.ElementAt(1).Key,
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            VerifyAllMocks(userReleaseRoleRepository, userRepository);
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
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository
                .Setup(m =>
                    m.GetByCompositeKey(
                        user.Id,
                        releaseVersion.Id,
                        ReleaseRole.PrereleaseViewer,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userReleaseRole);
            userReleaseRoleRepository
                .Setup(m => m.Remove(userReleaseRole, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemovePreReleaseUser(releaseVersion.Id, user.Email);

                result.AssertRight();
            }

            VerifyAllMocks(userReleaseRoleRepository, userRepository);
        }
    }

    private static PreReleaseUserService SetupPreReleaseUserService(
        ContentDbContext? contentDbContext = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            contentDbContext,
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService(_userId).Object,
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict)
        );
    }
}
