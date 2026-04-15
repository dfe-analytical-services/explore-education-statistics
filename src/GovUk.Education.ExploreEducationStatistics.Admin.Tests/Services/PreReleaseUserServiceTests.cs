#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
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

            var userPreReleaseRoles = _dataFixture
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
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [.. userPreReleaseRoles]);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupService(
                    contentDbContext: context,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsers(releaseVersion.Id);

                var users = result.AssertRight();

                var expectedRoleEmailsOrderedByEmail = userPreReleaseRoles
                    .Where(urr => urr.ReleaseVersion.Id == releaseVersion.Id)
                    .Select(urr => urr.User.Email)
                    .Order()
                    .ToList();

                var resultEmails = users.Select(u => u.Email).ToList();

                Assert.Equal(expectedRoleEmailsOrderedByEmail, resultEmails);
            }

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository);
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
                var service = SetupService(context);
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
            var service = SetupService(context);
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
                var service = SetupService(
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

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var usersWithPendingInviteAndPreReleaseRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndNoRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var activeUsersWithPreReleaseRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithNoRole = _dataFixture.DefaultUser().GenerateList(2);

            var usersWithPendingInviteAndPreReleaseRoleByEmail = usersWithPendingInviteAndPreReleaseRole.ToDictionary(
                u => u.Email
            );
            var usersWithPendingInviteAndNoRoleEmail = usersWithPendingInviteAndNoRole.ToDictionary(u => u.Email);
            var activeUsersWithPreReleaseRoleByEmail = activeUsersWithPreReleaseRole.ToDictionary(u => u.Email);
            var activeUsersWithNoRoleEmail = activeUsersWithNoRole.ToDictionary(u => u.Email);

            var allExistingUsersByEmail = usersWithPendingInviteAndPreReleaseRole
                .Concat(usersWithPendingInviteAndNoRole)
                .Concat(activeUsersWithPreReleaseRole)
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
                    usersWithPendingInviteAndPreReleaseRoleByEmail.TryGetValue(email, out var user)
                    || activeUsersWithPreReleaseRoleByEmail.TryGetValue(email, out user)
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
                var service = SetupService(
                    contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, allEmails);

                var plan = result.AssertRight();

                Assert.Equal(2, plan.AlreadyAccepted.Count);
                Assert.Equal(activeUsersWithPreReleaseRole[0].Email, plan.AlreadyAccepted[0]);
                Assert.Equal(activeUsersWithPreReleaseRole[1].Email, plan.AlreadyAccepted[1]);

                Assert.Equal(2, plan.AlreadyInvited.Count);
                Assert.Equal(usersWithPendingInviteAndPreReleaseRole[0].Email, plan.AlreadyInvited[0]);
                Assert.Equal(usersWithPendingInviteAndPreReleaseRole[1].Email, plan.AlreadyInvited[1]);

                Assert.Equal(6, plan.Invitable.Count);
                Assert.Equal("new.user.1@test.com", plan.Invitable[0]);
                Assert.Equal("new.user.2@test.com", plan.Invitable[1]);
                Assert.Equal(usersWithPendingInviteAndNoRole[0].Email, plan.Invitable[2]);
                Assert.Equal(usersWithPendingInviteAndNoRole[1].Email, plan.Invitable[3]);
                Assert.Equal(activeUsersWithNoRole[0].Email, plan.Invitable[4]);
                Assert.Equal(activeUsersWithNoRole[1].Email, plan.Invitable[5]);
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }
    }

    public class GrantPreReleaseAccessForMultipleUsersTests : PreReleaseUserServiceTests
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
                var service = SetupService(context);
                var result = await service.GrantPreReleaseAccessForMultipleUsers(
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
            var service = SetupService(context);
            var result = await service.GrantPreReleaseAccessForMultipleUsers(Guid.NewGuid(), ["test@test.com"]);

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
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(
                    releaseVersion.Id,
                    [activeUser.Email, userWithPendingInvite.Email]
                );

                result.AssertBadRequest(NoInvitableEmails);
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
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
                var service = SetupService(
                    contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(releaseVersion.Id, allEmails);

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

            MockUtils.VerifyAllMocks(userResourceRoleNotificationService, userPrereleaseRoleRepository, userRepository);
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
                var service = SetupService(
                    contentDbContext,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(releaseVersion.Id, allEmails);

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

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository, userRepository);
        }
    }

    public class GrantPreReleaseAccessTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task ApprovedLatestReleaseVersion_AdditionallySendsNotificationEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            User user = _dataFixture.DefaultUser();

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions[0];

            UserReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(latestReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        user.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPrereleaseRoleRepository
                .Setup(s => s.Create(user.Id, latestReleaseVersion.Id, user.Id, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUserPreReleaseRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPreReleaseRole(createdUserPreReleaseRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository, userResourceRoleNotificationService);
        }

        [Fact]
        public async Task DraftLatestReleaseVersion_DoesNotSendNotificationEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

            User user = _dataFixture.DefaultUser();

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions.Single(rv => rv.ApprovalStatus == ReleaseApprovalStatus.Draft);

            UserReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(latestReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        user.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPrereleaseRoleRepository
                .Setup(s => s.Create(user.Id, latestReleaseVersion.Id, user.Id, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUserPreReleaseRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task UserNotActive_AdditionallyUpdatesTheUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            User existingUser = _dataFixture.DefaultUser().WithActive(false);

            User updatedUser = _dataFixture.DefaultUserWithPendingInvite();

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions[0];

            UserReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(latestReleaseVersion);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindUserById(existingUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        existingUser.Email,
                        Role.PrereleaseUser,
                        existingUser.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(updatedUser);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        existingUser.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPrereleaseRoleRepository
                .Setup(s =>
                    s.Create(
                        updatedUser.Id,
                        latestReleaseVersion.Id,
                        updatedUser.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdUserPreReleaseRole);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPreReleaseRole(createdUserPreReleaseRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: existingUser.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository, userResourceRoleNotificationService);
        }

        [Fact]
        public async Task UserAlreadyHasPreReleaseRoleOnRelease_ReturnsBadRequest()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            User user = _dataFixture.DefaultUser();

            var release = publication.Releases.Single();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPrereleaseRoleOnReleaseVersion(
                        user.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.ActiveOnly,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task NoUser_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var userId = Guid.NewGuid();

            var release = publication.Releases.Single();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(mock => mock.FindUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext, userRepository: userRepository.Object);

                var result = await service.GrantPreReleaseAccess(userId: userId, releaseId: release.Id);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task NoReleaseVersionsExistForRelease_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 0)]);

            var userId = Guid.NewGuid();

            var release = publication.Releases.Single();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext);

                var result = await service.GrantPreReleaseAccess(userId: userId, releaseId: release.Id);

                result.AssertNotFound();
            }
        }
    }

    public class RemovePreReleaseRoleByCompositeKeyTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();
            UserReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            var result = await service.RemovePreReleaseRoleByCompositeKey(
                userPreReleaseRole.ReleaseVersionId,
                user.Email
            );

            result.AssertRight();

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNotFound()
        {
            User user = _dataFixture.DefaultUser();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, []);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            var result = await service.RemovePreReleaseRoleByCompositeKey(Guid.NewGuid(), user.Email);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_Throws()
        {
            User user = _dataFixture.DefaultUser();
            UserReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemovePreReleaseRoleByCompositeKey(userPreReleaseRole.ReleaseVersionId, user.Email)
            );

            MockUtils.VerifyAllMocks(userRepository, userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task InvalidEmail_ReturnsBadRequest()
        {
            var service = SetupService();
            var result = await service.RemovePreReleaseRoleByCompositeKey(Guid.NewGuid(), "not an email");

            result.AssertBadRequest(InvalidEmailAddress);
        }
    }

    public class RemovePreReleaseRoleTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            UserReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = SetupService(userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object);

            var result = await service.RemovePreReleaseRole(userPreReleaseRole.Id);

            result.AssertRight();

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNotFound()
        {
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, []);

            var service = SetupService(userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object);

            var result = await service.RemovePreReleaseRole(Guid.NewGuid());

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_Throws()
        {
            UserReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPrereleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = SetupService(userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemovePreReleaseRole(userPreReleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository);
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

            var service = SetupService(
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

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository, userRepository, releaseVersionRepository);
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNotFound()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = SetupService(userRepository: userRepository.Object);

            var result = await service.GetPrereleaseRolesForUser(userId);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userRepository);
        }
    }

    private static PreReleaseUserService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserPrereleaseRoleRepository? userPrereleaseRoleRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new(
            contentDbContext,
            usersAndRolesDbContext,
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? MockUtils.AlwaysTrueUserService(_userId).Object,
            userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            userPrereleaseRoleRepository ?? Mock.Of<IUserPrereleaseRoleRepository>(MockBehavior.Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict)
        );
    }
}
