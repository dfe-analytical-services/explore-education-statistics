#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

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

            var userPrereleaseRoles = _dataFixture
                // These should be returned, regardless of whether the user is active, pending or expired
                .DefaultUserPrereleaseRole()
                .ForIndex(0, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion))
                .ForIndex(
                    1,
                    s => s.SetUser(_dataFixture.DefaultUserWithPendingInvite()).SetReleaseVersion(releaseVersion)
                )
                .ForIndex(
                    2,
                    s => s.SetUser(_dataFixture.DefaultUserWithExpiredInvite()).SetReleaseVersion(releaseVersion)
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
                )
                .GenerateList(4);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>();
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [.. userPrereleaseRoles]);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext: context,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();

                var expectedRoleEmailsOrderedByEmail = userPrereleaseRoles
                    .Where(urr => urr.ReleaseVersion.Id == releaseVersion.Id)
                    .Select(urr => urr.User.Email)
                    .Order()
                    .ToList();

                var resultEmails = users.Select(u => u.Email).ToList();

                Assert.Equal(expectedRoleEmailsOrderedByEmail, resultEmails);
            }

            VerifyAllMocks(userPrereleaseRoleRepository);
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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        activeUser.Id,
                        releaseVersion.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        userWithPendingInvite.Id,
                        releaseVersion.Id,
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
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(
                    releaseVersion.Id,
                    [activeUser.Email, userWithPendingInvite.Email]
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
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
                    userPrereleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasPrereleaseRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
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

                userPrereleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPrereleaseRoleOnReleaseVersion(
                            userWithoutRole.Id,
                            releaseVersion.Id,
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
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
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

            VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
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
                    ["test1@test.com", "not an email", "test2@test.com"]
                );

                result.AssertBadRequest(InvalidEmailAddress);
            }
        }

        [Fact]
        public async Task NoRelease_ReturnsNotFound()
        {
            await using var context = InMemoryApplicationDbContext();
            var service = SetupPreReleaseUserService(context);
            var result = await service.InvitePreReleaseUsers(Guid.NewGuid(), ["test@test.com"]);

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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        activeUser.Id,
                        releaseVersion.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        userWithPendingInvite.Id,
                        releaseVersion.Id,
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
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.InvitePreReleaseUsers(
                    releaseVersion.Id,
                    [activeUser.Email, userWithPendingInvite.Email]
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
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

            // Although this looks similar to 'activeUsersWithNoRolesOrInvitesByEmail' below, they each have
            // unique setups when mocking the dependencies and behaviour below.
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

            var createUserPrereleaseRoleFunc = (string email) =>
                _dataFixture
                    .DefaultUserPrereleaseRole()
                    .WithUserId(
                        existingUsersByEmail.TryGetValue(email, out var user) ? user.Id : newUserIdsByEmail[email]
                    )
                    .WithReleaseVersion(releaseVersion)
                    .Generate();

            var createdUserPrereleaseRolesByEmail = new Dictionary<string, UserReleaseRole>();

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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRoleByEmail.TryGetValue(email, out var user)
                    || usersWithPendingInvitesAndExistingRoleByEmail.TryGetValue(email, out user)
                )
                {
                    userPrereleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasPrereleaseRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(true);

                    continue;
                }

                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userPrereleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasPrereleaseRoleOnReleaseVersion(
                                existingUser.Id,
                                releaseVersion.Id,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var createdUserPrereleaseRole = createUserPrereleaseRoleFunc(email);
                createdUserPrereleaseRolesByEmail.Add(email, createdUserPrereleaseRole);

                var userId = existingUser?.Id ?? newUserIdsByEmail[email];
                userPrereleaseRoleRepository
                    .Setup(mock => mock.Create(userId, releaseVersion.Id, _userId, null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdUserPrereleaseRole);
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

                var createdUserPrereleaseRole = createdUserPrereleaseRolesByEmail[email];

                userResourceRoleNotificationService
                    .Setup(mock =>
                        mock.NotifyUserOfNewPreReleaseRole(createdUserPrereleaseRole.Id, It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
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

            VerifyAllMocks(userResourceRoleNotificationService, userPrereleaseRoleRepository, userRepository);
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

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            foreach (var email in allEmails)
            {
                if (
                    usersWithExistingRoleByEmail.TryGetValue(email, out var user)
                    || usersWithPendingInvitesAndExistingRoleByEmail.TryGetValue(email, out user)
                )
                {
                    userPrereleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasPrereleaseRoleOnReleaseVersion(
                                user.Id,
                                releaseVersion.Id,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(true);

                    continue;
                }

                if (existingUsersByEmail.TryGetValue(email, out var existingUser))
                {
                    userPrereleaseRoleRepository
                        .Setup(mock =>
                            mock.UserHasPrereleaseRoleOnReleaseVersion(
                                existingUser.Id,
                                releaseVersion.Id,
                                ResourceRoleFilter.AllButExpired,
                                It.IsAny<CancellationToken>()
                            )
                        )
                        .ReturnsAsync(false);
                }

                var userId = existingUser?.Id ?? newUserIdsByEmail[email];
                userPrereleaseRoleRepository
                    .Setup(mock => mock.Create(userId, releaseVersion.Id, _userId, null, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        _dataFixture
                            .DefaultUserPrereleaseRole()
                            .WithUser(_dataFixture.DefaultUser())
                            .WithReleaseVersion(
                                _dataFixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                    )
                            )
                            .Generate()
                    );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
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

            VerifyAllMocks(userPrereleaseRoleRepository, userRepository);
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
            UserReleaseRole userPrereleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(m =>
                    m.RemoveByCompositeKey(
                        userPrereleaseRole.UserId,
                        userPrereleaseRole.ReleaseVersionId,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemovePreReleaseUser(releaseVersion.Id, user.Email);

                result.AssertRight();
            }

            VerifyAllMocks(userPrereleaseRoleRepository, userRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_ReturnsNotFound()
        {
            User user = _dataFixture.DefaultUser();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
            UserReleaseRole userPrereleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(m =>
                    m.RemoveByCompositeKey(
                        userPrereleaseRole.UserId,
                        userPrereleaseRole.ReleaseVersionId,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupPreReleaseUserService(
                    contentDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemovePreReleaseUser(releaseVersion.Id, user.Email);

                result.AssertNotFound();
            }

            VerifyAllMocks(userPrereleaseRoleRepository, userRepository);
        }
    }

    public class GetPrereleaseRolesForUserTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();

            var (publication1, publication2, publication3, publication4) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)])
                .GenerateTuple4();

            UserReleaseRole userPrereleaseRole1 = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(publication1.Releases[0].Versions[1])
                .WithUser(user);

            UserReleaseRole userPrereleaseRole2 = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(publication2.Releases[0].Versions[1])
                .WithUser(user);

            // Role assignment for a non-latest release version, which should be filtered out of the results
            UserReleaseRole userPrereleaseRole3 = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(publication3.Releases[0].Versions[0])
                .WithUser(user);

            // Role assignment for a different user, which should be filtered out of the results
            UserReleaseRole userPrereleaseRole4 = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(publication3.Releases[0].Versions[1])
                .WithUser(_dataFixture.DefaultUser().Generate());

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindActiveUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(
                ResourceRoleFilter.ActiveOnly,
                [userPrereleaseRole1, userPrereleaseRole2, userPrereleaseRole3, userPrereleaseRole4]
            );

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPrereleaseRole1.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(true);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPrereleaseRole2.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(true);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPrereleaseRole3.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(false);

            var service = SetupPreReleaseUserService(
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                userRepository: userRepository.Object,
                releaseVersionRepository: releaseVersionRepository.Object
            );

            var result = await service.GetPrereleaseRolesForUser(user.Id);

            var userReleaseRoles = result.AssertRight();

            Assert.Equal(2, userReleaseRoles.Count);

            Assert.Equal(userPrereleaseRole1.Id, userReleaseRoles[0].Id);
            Assert.Equal(publication1.Title, userReleaseRoles[0].Publication);
            Assert.Equal(publication1.Releases[0].Title, userReleaseRoles[0].Release);

            Assert.Equal(userPrereleaseRole2.Id, userReleaseRoles[1].Id);
            Assert.Equal(publication2.Title, userReleaseRoles[1].Publication);
            Assert.Equal(publication2.Releases[0].Title, userReleaseRoles[1].Release);

            VerifyAllMocks(userPrereleaseRoleRepository, userRepository, releaseVersionRepository);
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupPreReleaseUserService(userRepository: userRepository.Object);

            var result = await service.GetPrereleaseRolesForUser(userId);

            result.AssertNotFound();

            VerifyAllMocks(userRepository);
        }
    }

    private static PreReleaseUserService SetupPreReleaseUserService(
        ContentDbContext? contentDbContext = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserPrereleaseRoleRepository? userPrereleaseRoleRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            contentDbContext,
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? AlwaysTrueUserService(_userId).Object,
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userPrereleaseRoleRepository ?? Mock.Of<IUserPrereleaseRoleRepository>(MockBehavior.Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict)
        );
    }
}
