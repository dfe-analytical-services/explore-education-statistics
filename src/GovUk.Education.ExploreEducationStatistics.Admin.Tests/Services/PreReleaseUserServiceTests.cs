#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;
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

    public static readonly TheoryData<Func<DataFixture, User>> AllTypesOfNonActiveUser =
    [
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
        // Soft Deleted User
        fixture => fixture.DefaultSoftDeletedUser(),
    ];

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
                .DefaultUserPreReleaseRole()
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

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>();
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [.. userPreReleaseRoles]);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupService(
                    contentDbContext: context,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
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

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);
        }
    }

    public class GetPreReleaseUsersInvitePlanTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task NoRelease_ReturnsNotFound()
        {
            var request = new PreReleaseUserInviteRequest { Emails = ListOf("test@test.com") };

            await using var context = InMemoryApplicationDbContext();
            var service = SetupService(context);
            var result = await service.GetPreReleaseUsersInvitePlan(Guid.NewGuid(), request);

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

            var request = new PreReleaseUserInviteRequest { Emails = [activeUser.Email, userWithPendingInvite.Email] };

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

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        activeUser.Id,
                        releaseVersion.Release.PublicationId,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        userWithPendingInvite.Id,
                        releaseVersion.Release.PublicationId,
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
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, request);

                result.AssertBadRequest(NoInvitableEmails);
            }

            MockUtils.VerifyAllMocks(userRepository, userPublicationRoleRepository);
        }

        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

            var usersWithPendingInviteAndPublicationRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndPreReleaseRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndNoRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var activeUsersWithPublicationRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithPreReleaseRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithNoRole = _dataFixture.DefaultUser().GenerateList(2);
            var softDeletedUsersWithNoRole = _dataFixture.DefaultSoftDeletedUser().GenerateList(2);
            var expiredUsersWithNoRole = _dataFixture.DefaultUserWithExpiredInvite().GenerateList(2);

            var allExistingUsers = usersWithPendingInviteAndPublicationRole
                .Concat(usersWithPendingInviteAndPreReleaseRole)
                .Concat(usersWithPendingInviteAndNoRole)
                .Concat(activeUsersWithPublicationRole)
                .Concat(activeUsersWithPreReleaseRole)
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .ToHashSet();

            var existingUsersWithPublicationRole = usersWithPendingInviteAndPublicationRole.Concat(
                activeUsersWithPublicationRole
            );
            var existingUsersWithoutPublicationRole = allExistingUsers.Except(existingUsersWithPublicationRole);
            var existingUsersWithPreReleaseRole = usersWithPendingInviteAndPreReleaseRole.Concat(
                activeUsersWithPreReleaseRole
            );
            var existingUsersWithoutAnyRole = usersWithPendingInviteAndNoRole
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole);

            var newEmails = ListOf("new.user.1@test.com", "new.user.2@test.com");

            var allEmails = newEmails.Concat(allExistingUsers.Select(eu => eu.Email)).ToList();

            var request = new PreReleaseUserInviteRequest { Emails = allEmails };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);

            foreach (var existingUser in allExistingUsers)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingUser);
            }

            foreach (var newEmail in newEmails)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(newEmail, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            foreach (var existingUserWithPublicationRole in existingUsersWithPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutPublicationRole in existingUsersWithoutPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithoutPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            foreach (var existingUserWithPreReleaseRole in existingUsersWithPreReleaseRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithPreReleaseRole.Id,
                            releaseVersion.Id,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutAnyRole in existingUsersWithoutAnyRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithoutAnyRole.Id,
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
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GetPreReleaseUsersInvitePlan(releaseVersion.Id, request);

                var plan = result.AssertRight();

                Assert.Equal(2, plan.AlreadyAccepted.Count);
                Assert.Equal(activeUsersWithPreReleaseRole.Select(u => u.Email), plan.AlreadyAccepted);

                Assert.Equal(2, plan.AlreadyInvited.Count);
                Assert.Equal(usersWithPendingInviteAndPreReleaseRole.Select(u => u.Email), plan.AlreadyInvited);

                Assert.Equal(4, plan.AlreadyHasMorePowerfulRole.Count);
                Assert.Equal(existingUsersWithPublicationRole.Select(u => u.Email), plan.AlreadyHasMorePowerfulRole);

                Assert.Equal(10, plan.Invitable.Count);
                Assert.Equal(newEmails.Concat(existingUsersWithoutAnyRole.Select(u => u.Email)), plan.Invitable);
            }

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository, userPublicationRoleRepository);
        }
    }

    public class GrantPreReleaseAccessForMultipleUsersTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task NoRelease_ReturnsNotFound()
        {
            var request = new PreReleaseUserInviteRequest { Emails = ["test@test.com"] };

            await using var context = InMemoryApplicationDbContext();
            var service = SetupService(context);
            var result = await service.GrantPreReleaseAccessForMultipleUsers(Guid.NewGuid(), request);

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

            var request = new PreReleaseUserInviteRequest { Emails = [activeUser.Email, userWithPendingInvite.Email] };

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

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        activeUser.Id,
                        releaseVersion.Release.PublicationId,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(true);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        userWithPendingInvite.Id,
                        releaseVersion.Release.PublicationId,
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
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(releaseVersion.Id, request);

                result.AssertBadRequest(NoInvitableEmails);
            }

            MockUtils.VerifyAllMocks(userRepository, userPublicationRoleRepository);
        }

        [Fact]
        public async Task InvitesMultipleUsersForApprovedRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublishScheduled(PublishedScheduledStartOfDay);

            var newEmails = ListOf("new.user.1@test.com", "new.user.2@test.com");

            var newUserIdEmailPairs = newEmails.Select(e => (Id: Guid.NewGuid(), Email: e)).ToList();

            var usersWithPendingInviteAndPublicationRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndPreReleaseRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndNoRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var activeUsersWithPublicationRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithPreReleaseRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithNoRole = _dataFixture.DefaultUser().GenerateList(2);
            var softDeletedUsersWithNoRole = _dataFixture.DefaultSoftDeletedUser().GenerateList(2);
            var expiredUsersWithNoRole = _dataFixture.DefaultUserWithExpiredInvite().GenerateList(2);

            var allExistingUsers = usersWithPendingInviteAndPublicationRole
                .Concat(usersWithPendingInviteAndPreReleaseRole)
                .Concat(usersWithPendingInviteAndNoRole)
                .Concat(activeUsersWithPublicationRole)
                .Concat(activeUsersWithPreReleaseRole)
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .ToHashSet();

            var usersThatNeedCreatingOrRecreatingIdEmailPairs = usersWithPendingInviteAndNoRole
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .Select(u => (u.Id, u.Email))
                .Concat(newUserIdEmailPairs)
                .ToList();
            var existingUsersWithPublicationRole = usersWithPendingInviteAndPublicationRole.Concat(
                activeUsersWithPublicationRole
            );
            var existingUsersWithoutPublicationRole = allExistingUsers.Except(existingUsersWithPublicationRole);
            var existingUsersWithPreReleaseRole = usersWithPendingInviteAndPreReleaseRole.Concat(
                activeUsersWithPreReleaseRole
            );
            var existingUsersWithoutAnyRole = usersWithPendingInviteAndNoRole
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole);
            var userIdsThatRequireAPreReleaseRoleAdding = usersWithPendingInviteAndNoRole
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .Select(u => u.Id)
                .Concat(newUserIdEmailPairs.Select(pair => pair.Id));

            var allEmails = newEmails.Concat(allExistingUsers.Select(eu => eu.Email)).ToList();

            var activeApplicationUsersWithNoRole = activeUsersWithNoRole
                .Select(u => new ApplicationUser { Id = u.Id.ToString(), Email = u.Email })
                .ToList();

            var request = new PreReleaseUserInviteRequest { Emails = allEmails };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.AddRange(activeApplicationUsersWithNoRole);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var createdUserPreReleaseRoles = new List<UserPreReleaseRole>();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);

            foreach (var existingUser in allExistingUsers)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingUser);
            }

            foreach (var newEmail in newEmails)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(newEmail, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            foreach (var activeUserWithNoRole in activeUsersWithNoRole)
            {
                userRepository
                    .Setup(mock =>
                        mock.FindActiveUserByEmail(activeUserWithNoRole.Email, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(activeUserWithNoRole);
            }

            foreach (var (Id, Email) in usersThatNeedCreatingOrRecreatingIdEmailPairs)
            {
                userRepository
                    .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

                userRepository
                    .Setup(mock =>
                        mock.CreateOrUpdate(
                            Email,
                            GlobalRoles.Role.PrereleaseUser,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(
                        _dataFixture
                            .DefaultUserWithPendingInvite()
                            .WithId(Id)
                            .WithEmail(Email)
                            .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
                            .WithCreatedById(_userId)
                            .WithCreated(DateTimeOffset.UtcNow)
                    );
            }

            foreach (var existingUserWithPublicationRole in existingUsersWithPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutPublicationRole in existingUsersWithoutPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithoutPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            foreach (var existingUserWithPreReleaseRole in existingUsersWithPreReleaseRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithPreReleaseRole.Id,
                            releaseVersion.Id,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutAnyRole in existingUsersWithoutAnyRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithoutAnyRole.Id,
                            releaseVersion.Id,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            foreach (var userIdThatRequiresAPreReleaseRoleAdding in userIdsThatRequireAPreReleaseRoleAdding)
            {
                var createdUserPreReleaseRole = _dataFixture
                    .DefaultUserPreReleaseRole()
                    .WithUserId(userIdThatRequiresAPreReleaseRoleAdding)
                    .WithReleaseVersion(releaseVersion)
                    .Generate();

                createdUserPreReleaseRoles.Add(createdUserPreReleaseRole);

                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.Create(
                            userIdThatRequiresAPreReleaseRoleAdding,
                            releaseVersion.Id,
                            _userId,
                            default,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(createdUserPreReleaseRole);
            }

            foreach (var createdUserPreReleaseRole in createdUserPreReleaseRoles)
            {
                userResourceRoleNotificationService
                    .Setup(mock =>
                        mock.NotifyUserOfNewPreReleaseRole(createdUserPreReleaseRole.Id, It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            foreach (var activeApplicationUserWithNoRole in activeApplicationUsersWithNoRole)
            {
                globalRoleService
                    .Setup(mock =>
                        mock.UpgradeToGlobalRoleIfRequired(
                            ItIsApplicationUser(activeApplicationUserWithNoRole),
                            RoleNames.PrereleaseUser
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(releaseVersion.Id, request);

                var preReleaseUsers = result.AssertRight();

                // The users that already have the release role or invite should be filtered out
                Assert.Equal(10, preReleaseUsers.Count);

                var expectedEmails = new[]
                {
                    usersWithPendingInviteAndNoRole.ElementAt(0).Email,
                    usersWithPendingInviteAndNoRole.ElementAt(1).Email,
                    activeUsersWithNoRole.ElementAt(0).Email,
                    activeUsersWithNoRole.ElementAt(1).Email,
                    softDeletedUsersWithNoRole.ElementAt(0).Email,
                    softDeletedUsersWithNoRole.ElementAt(1).Email,
                    expiredUsersWithNoRole.ElementAt(0).Email,
                    expiredUsersWithNoRole.ElementAt(1).Email,
                    newUserIdEmailPairs.ElementAt(0).Email,
                    newUserIdEmailPairs.ElementAt(1).Email,
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            MockUtils.VerifyAllMocks(
                userResourceRoleNotificationService,
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                globalRoleService
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

            var newEmails = ListOf("new.user.1@test.com", "new.user.2@test.com");

            var newUserIdEmailPairs = newEmails.Select(e => (Id: Guid.NewGuid(), Email: e)).ToList();

            var usersWithPendingInviteAndPublicationRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndPreReleaseRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var usersWithPendingInviteAndNoRole = _dataFixture.DefaultUserWithPendingInvite().GenerateList(2);
            var activeUsersWithPublicationRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithPreReleaseRole = _dataFixture.DefaultUser().GenerateList(2);
            var activeUsersWithNoRole = _dataFixture.DefaultUser().GenerateList(2);
            var softDeletedUsersWithNoRole = _dataFixture.DefaultSoftDeletedUser().GenerateList(2);
            var expiredUsersWithNoRole = _dataFixture.DefaultUserWithExpiredInvite().GenerateList(2);

            var allExistingUsers = usersWithPendingInviteAndPublicationRole
                .Concat(usersWithPendingInviteAndPreReleaseRole)
                .Concat(usersWithPendingInviteAndNoRole)
                .Concat(activeUsersWithPublicationRole)
                .Concat(activeUsersWithPreReleaseRole)
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .ToHashSet();

            var usersThatNeedCreatingOrRecreatingIdEmailPairs = usersWithPendingInviteAndNoRole
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .Select(u => (u.Id, u.Email))
                .Concat(newUserIdEmailPairs)
                .ToList();
            var existingUsersWithPublicationRole = usersWithPendingInviteAndPublicationRole.Concat(
                activeUsersWithPublicationRole
            );
            var existingUsersWithoutPublicationRole = allExistingUsers.Except(existingUsersWithPublicationRole);
            var existingUsersWithPreReleaseRole = usersWithPendingInviteAndPreReleaseRole.Concat(
                activeUsersWithPreReleaseRole
            );
            var existingUsersWithoutAnyRole = usersWithPendingInviteAndNoRole
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole);
            var userIdsThatRequireAPreReleaseRoleAdding = usersWithPendingInviteAndNoRole
                .Concat(activeUsersWithNoRole)
                .Concat(softDeletedUsersWithNoRole)
                .Concat(expiredUsersWithNoRole)
                .Select(u => u.Id)
                .Concat(newUserIdEmailPairs.Select(pair => pair.Id));

            var allEmails = newEmails.Concat(allExistingUsers.Select(eu => eu.Email)).ToList();

            var activeApplicationUsersWithNoRole = activeUsersWithNoRole
                .Select(u => new ApplicationUser { Id = u.Id.ToString(), Email = u.Email })
                .ToList();

            var request = new PreReleaseUserInviteRequest { Emails = allEmails };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.AddRange(activeApplicationUsersWithNoRole);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var createdUserPreReleaseRoles = new List<UserPreReleaseRole>();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);

            foreach (var existingUser in allExistingUsers)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(existingUser.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existingUser);
            }

            foreach (var newEmail in newEmails)
            {
                userRepository
                    .Setup(mock => mock.FindUserByEmail(newEmail, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            foreach (var activeUserWithNoRole in activeUsersWithNoRole)
            {
                userRepository
                    .Setup(mock =>
                        mock.FindActiveUserByEmail(activeUserWithNoRole.Email, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(activeUserWithNoRole);
            }

            foreach (var (Id, Email) in usersThatNeedCreatingOrRecreatingIdEmailPairs)
            {
                userRepository
                    .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

                userRepository
                    .Setup(mock =>
                        mock.CreateOrUpdate(
                            Email,
                            GlobalRoles.Role.PrereleaseUser,
                            _userId,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(
                        _dataFixture
                            .DefaultUserWithPendingInvite()
                            .WithId(Id)
                            .WithEmail(Email)
                            .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
                            .WithCreatedById(_userId)
                            .WithCreated(DateTimeOffset.UtcNow)
                    );
            }

            foreach (var existingUserWithPublicationRole in existingUsersWithPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutPublicationRole in existingUsersWithoutPublicationRole)
            {
                userPublicationRoleRepository
                    .Setup(mock =>
                        mock.UserHasAnyRoleOnPublication(
                            existingUserWithoutPublicationRole.Id,
                            releaseVersion.Release.PublicationId,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            foreach (var existingUserWithPreReleaseRole in existingUsersWithPreReleaseRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithPreReleaseRole.Id,
                            releaseVersion.Id,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(true);
            }

            foreach (var existingUserWithoutAnyRole in existingUsersWithoutAnyRole)
            {
                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.UserHasPreReleaseRoleOnReleaseVersion(
                            existingUserWithoutAnyRole.Id,
                            releaseVersion.Id,
                            ResourceRoleFilter.AllButExpired,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(false);
            }

            foreach (var userIdThatRequiresAPreReleaseRoleAdding in userIdsThatRequireAPreReleaseRoleAdding)
            {
                var createdUserPreReleaseRole = _dataFixture
                    .DefaultUserPreReleaseRole()
                    .WithUserId(userIdThatRequiresAPreReleaseRoleAdding)
                    .WithReleaseVersion(releaseVersion)
                    .Generate();

                createdUserPreReleaseRoles.Add(createdUserPreReleaseRole);

                userPreReleaseRoleRepository
                    .Setup(mock =>
                        mock.Create(
                            userIdThatRequiresAPreReleaseRoleAdding,
                            releaseVersion.Id,
                            _userId,
                            default,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .ReturnsAsync(createdUserPreReleaseRole);
            }

            foreach (var activeApplicationUserWithNoRole in activeApplicationUsersWithNoRole)
            {
                globalRoleService
                    .Setup(mock =>
                        mock.UpgradeToGlobalRoleIfRequired(
                            ItIsApplicationUser(activeApplicationUserWithNoRole),
                            RoleNames.PrereleaseUser
                        )
                    )
                    .Returns(Task.CompletedTask);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userRepository: userRepository.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.GrantPreReleaseAccessForMultipleUsers(releaseVersion.Id, request);

                var preReleaseUsers = result.AssertRight();

                // The users that already have the release role or invite should be filtered out
                Assert.Equal(10, preReleaseUsers.Count);

                var expectedEmails = new[]
                {
                    usersWithPendingInviteAndNoRole.ElementAt(0).Email,
                    usersWithPendingInviteAndNoRole.ElementAt(1).Email,
                    activeUsersWithNoRole.ElementAt(0).Email,
                    activeUsersWithNoRole.ElementAt(1).Email,
                    softDeletedUsersWithNoRole.ElementAt(0).Email,
                    softDeletedUsersWithNoRole.ElementAt(1).Email,
                    expiredUsersWithNoRole.ElementAt(0).Email,
                    expiredUsersWithNoRole.ElementAt(1).Email,
                    newUserIdEmailPairs.ElementAt(0).Email,
                    newUserIdEmailPairs.ElementAt(1).Email,
                };

                var preReleaseEmails = preReleaseUsers.Select(u => u.Email).ToList();

                Assert.All(expectedEmails, email => Assert.Contains(email, preReleaseEmails));
            }

            MockUtils.VerifyAllMocks(
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userRepository,
                globalRoleService
            );
        }
    }

    public class GrantPreReleaseAccessTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task ActiveUser_ApprovedLatestReleaseVersion_AdditionallySendsNotificationEmail_AttemptsToUpgradeGlobalRole()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            User user = _dataFixture.DefaultUser();
            var applicationUser = new ApplicationUser { Id = user.Id.ToString(), Email = user.Email };

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions[0];

            UserPreReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(latestReleaseVersion);

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(applicationUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        user.Id,
                        latestReleaseVersion.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPreReleaseRoleRepository
                .Setup(s => s.Create(user.Id, latestReleaseVersion.Id, _userId, default, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUserPreReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        user.Id,
                        publication.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService
                .Setup(mock =>
                    mock.UpgradeToGlobalRoleIfRequired(ItIsApplicationUser(applicationUser), RoleNames.PrereleaseUser)
                )
                .Returns(Task.CompletedTask);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(
                MockBehavior.Strict
            );
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewPreReleaseRole(createdUserPreReleaseRole.Id, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRepository: userRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(
                userRepository,
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userResourceRoleNotificationService,
                globalRoleService
            );
        }

        [Fact]
        public async Task ActiveUser_DraftLatestReleaseVersion_DoesNotSendNotificationEmail_AttemptsToUpgradeGlobalRole()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

            User user = _dataFixture.DefaultUser();
            var applicationUser = new ApplicationUser { Id = user.Id.ToString(), Email = user.Email };

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions.Single(rv => rv.ApprovalStatus == ReleaseApprovalStatus.Draft);

            UserPreReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(latestReleaseVersion);

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(applicationUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(mock => mock.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        user.Id,
                        latestReleaseVersion.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPreReleaseRoleRepository
                .Setup(s => s.Create(user.Id, latestReleaseVersion.Id, _userId, default, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUserPreReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        user.Id,
                        publication.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService
                .Setup(mock =>
                    mock.UpgradeToGlobalRoleIfRequired(ItIsApplicationUser(applicationUser), RoleNames.PrereleaseUser)
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRepository: userRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(
                userRepository,
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                globalRoleService
            );
        }

        [Fact]
        public async Task UserNotActive_AdditionallyUpdatesTheUser_DoesNotAttemptToUpgradeGlobalRole()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            User existingUser = _dataFixture.DefaultUser().WithActive(false);

            User updatedUser = _dataFixture.DefaultUserWithPendingInvite();

            var release = publication.Releases.Single();
            var latestReleaseVersion = release.Versions[0];

            UserPreReleaseRole createdUserPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
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
                        _userId,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(updatedUser);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        existingUser.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
            userPreReleaseRoleRepository
                .Setup(s =>
                    s.Create(updatedUser.Id, latestReleaseVersion.Id, _userId, default, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(createdUserPreReleaseRole);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        existingUser.Id,
                        publication.Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

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
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: existingUser.Id, releaseId: release.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(
                userRepository,
                userPreReleaseRoleRepository,
                userPublicationRoleRepository,
                userResourceRoleNotificationService
            );
        }

        [Fact]
        public async Task UserAlreadyHasPreReleaseRoleOnReleaseVersion_ReturnsBadRequest()
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

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        user.Id,
                        release.Versions[0].Id,
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
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertBadRequest(UserAlreadyHasResourceRole);
            }

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task UserAlreadyHasMorePowerfulRoleOnReleaseVersion_ReturnsBadRequest()
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

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock =>
                    mock.UserHasPreReleaseRoleOnReleaseVersion(
                        user.Id,
                        release.Versions[0].Id,
                        ResourceRoleFilter.AllButExpired,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock =>
                    mock.UserHasAnyRoleOnPublication(
                        user.Id,
                        publication.Id,
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
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.GrantPreReleaseAccess(userId: user.Id, releaseId: release.Id);

                result.AssertBadRequest(UserAlreadyHasMorePowerfulRole);
            }

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository, userPublicationRoleRepository);
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

    public class RevokePreReleaseAccessByCompositeKeyTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task ActiveUser_AttemptsToDowngradeGlobalRole()
        {
            User user = _dataFixture.DefaultUser();
            var applicationUser = new ApplicationUser { Id = user.Id.ToString(), Email = user.Email };
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var request = new PreReleaseUserRemoveRequest { Email = userPreReleaseRole.User.Email };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(applicationUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindUserByEmail(userPreReleaseRole.User.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPreReleaseRole.User);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService
                .Setup(mock =>
                    mock.DowngradeFromGlobalRoleIfRequired(
                        ItIsApplicationUser(applicationUser),
                        RoleNames.PrereleaseUser
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRepository: userRepository.Object,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.RevokePreReleaseAccessByCompositeKey(
                    userPreReleaseRole.ReleaseVersionId,
                    request
                );

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository, globalRoleService);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfNonActiveUser))]
        public async Task NonActiveUser_DoesNotAttemptToDowngradeGlobalRole(Func<DataFixture, User> userFactory)
        {
            User user = userFactory(_dataFixture);
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var request = new PreReleaseUserRemoveRequest { Email = userPreReleaseRole.User.Email };

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindUserByEmail(userPreReleaseRole.User.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPreReleaseRole.User);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await service.RevokePreReleaseAccessByCompositeKey(
                userPreReleaseRole.ReleaseVersionId,
                request
            );

            result.AssertRight();

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNotFound()
        {
            User user = _dataFixture.DefaultUser();

            var request = new PreReleaseUserRemoveRequest { Email = user.Email };

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindUserByEmail(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, []);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            var result = await service.RevokePreReleaseAccessByCompositeKey(Guid.NewGuid(), request);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_Throws()
        {
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var request = new PreReleaseUserRemoveRequest { Email = userPreReleaseRole.User.Email };

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository
                .Setup(m => m.FindUserByEmail(userPreReleaseRole.User.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPreReleaseRole.User);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, userPreReleaseRole);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = SetupService(
                userRepository: userRepository.Object,
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RevokePreReleaseAccessByCompositeKey(userPreReleaseRole.ReleaseVersionId, request)
            );

            MockUtils.VerifyAllMocks(userRepository, userPreReleaseRoleRepository);
        }
    }

    public class RevokePreReleaseAccessByIdTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task ActiveUser_AttemptsToDowngradeGlobalRole()
        {
            User user = _dataFixture.DefaultUser();
            var applicationUser = new ApplicationUser { Id = user.Id.ToString(), Email = user.Email };
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                usersAndRolesDbContext.Users.Add(applicationUser);
                await usersAndRolesDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService
                .Setup(mock =>
                    mock.DowngradeFromGlobalRoleIfRequired(
                        ItIsApplicationUser(applicationUser),
                        RoleNames.PrereleaseUser
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupService(
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    globalRoleService: globalRoleService.Object
                );

                var result = await service.RevokePreReleaseAccessById(userPreReleaseRole.Id);

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, globalRoleService);
        }

        [Theory]
        [MemberData(nameof(AllTypesOfNonActiveUser))]
        public async Task NonActiveUser_DoesNotAttemptToDowngradeGlobalRole(Func<DataFixture, User> userFactory)
        {
            User user = userFactory(_dataFixture);
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = SetupService(userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object);

            var result = await service.RevokePreReleaseAccessById(userPreReleaseRole.Id);

            result.AssertRight();

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RoleDoesNotExist_ReturnsNotFound()
        {
            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, []);

            var service = SetupService(userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object);

            var result = await service.RevokePreReleaseAccessById(Guid.NewGuid());

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);
        }

        [Fact]
        public async Task RoleRemovalFails_Throws()
        {
            UserPreReleaseRole userPreReleaseRole = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(ResourceRoleFilter.All, [userPreReleaseRole]);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveById(userPreReleaseRole.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = SetupService(userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RevokePreReleaseAccessById(userPreReleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository);
        }
    }

    public class GetPreReleaseRolesForUserTests : PreReleaseUserServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();

            var (publication1, publication2, publication3, publication4) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)])
                .GenerateTuple4();

            UserPreReleaseRole userPreReleaseRole1 = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(publication1.Releases[0].Versions[1])
                .WithUser(user);

            UserPreReleaseRole userPreReleaseRole2 = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(publication2.Releases[0].Versions[1])
                .WithUser(user);

            // Role assignment for a non-latest release version, which should be filtered out of the results
            UserPreReleaseRole userPreReleaseRole3 = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(publication3.Releases[0].Versions[0])
                .WithUser(user);

            // Role assignment for a different user, which should be filtered out of the results
            UserPreReleaseRole userPreReleaseRole4 = _dataFixture
                .DefaultUserPreReleaseRole()
                .WithReleaseVersion(publication3.Releases[0].Versions[1])
                .WithUser(_dataFixture.DefaultUser().Generate());

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            userRepository.Setup(m => m.FindActiveUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository.SetupQuery(
                ResourceRoleFilter.ActiveOnly,
                [userPreReleaseRole1, userPreReleaseRole2, userPreReleaseRole3, userPreReleaseRole4]
            );

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPreReleaseRole1.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(true);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPreReleaseRole2.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(true);
            releaseVersionRepository
                .Setup(m =>
                    m.IsLatestReleaseVersion(userPreReleaseRole3.ReleaseVersionId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(false);

            var service = SetupService(
                userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                userRepository: userRepository.Object,
                releaseVersionRepository: releaseVersionRepository.Object
            );

            var result = await service.GetPreReleaseRolesForUser(user.Id);

            var userReleaseRoles = result.AssertRight();

            Assert.Equal(2, userReleaseRoles.Count);

            Assert.Equal(userPreReleaseRole1.Id, userReleaseRoles[0].Id);
            Assert.Equal(publication1.Title, userReleaseRoles[0].Publication);
            Assert.Equal(publication1.Releases[0].Title, userReleaseRoles[0].Release);

            Assert.Equal(userPreReleaseRole2.Id, userReleaseRoles[1].Id);
            Assert.Equal(publication2.Title, userReleaseRoles[1].Publication);
            Assert.Equal(publication2.Releases[0].Title, userReleaseRoles[1].Release);

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userRepository, releaseVersionRepository);
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

            var result = await service.GetPreReleaseRolesForUser(userId);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(userRepository);
        }
    }

    private static ApplicationUser ItIsApplicationUser(ApplicationUser applicationUser) =>
        It.Is<ApplicationUser>(au => au.Id == applicationUser.Id);

    private static PreReleaseUserService SetupService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IGlobalRoleService? globalRoleService = null
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
            userPreReleaseRoleRepository ?? Mock.Of<IUserPreReleaseRoleRepository>(MockBehavior.Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict),
            globalRoleService ?? Mock.Of<IGlobalRoleService>(MockBehavior.Strict)
        );
    }
}
