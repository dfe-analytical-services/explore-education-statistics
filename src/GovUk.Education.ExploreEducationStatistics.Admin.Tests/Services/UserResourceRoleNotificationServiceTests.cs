#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserResourceRoleNotificationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class NotifyUserOfInviteTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User inactiveUser = _dataFixture.DefaultUserWithPendingInvite();

            var publicationRolesForTargetUser = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(inactiveUser)
                .WithPublication(_dataFixture.DefaultPublication())
                .GenerateList(3);
            var prereleaseRolesForTargetUser = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(inactiveUser)
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .GenerateList(3);

            // These ones should be filtered out as they're for a different user
            var publicationRolesForOtherUser = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUserWithPendingInvite())
                .WithPublication(_dataFixture.DefaultPublication())
                .GenerateList(3);
            var prereleaseRolesForOtherUser = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUserWithPendingInvite())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .GenerateList(3);

            var allUserPublicationRoles = publicationRolesForTargetUser.Concat(publicationRolesForOtherUser).ToList();
            var allUserPrereleaseRoles = prereleaseRolesForTargetUser.Concat(prereleaseRolesForOtherUser).ToList();

            var prereleaseRolesInfo = prereleaseRolesForTargetUser
                .Select(urr => (urr.ReleaseVersion.Release.Publication.Title, urr.ReleaseVersion.Release.Title))
                .ToHashSet();

            var publicationRolesInfo = publicationRolesForTargetUser
                .Select(upr => (upr.Publication.Title, upr.Role))
                .ToHashSet();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);
            // Set up the current time in UTC
            var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

            userRepository
                .Setup(r => r.FindUserById(inactiveUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            userPublicationRoleRepository.SetupQuery(
                ResourceRoleFilter.PendingOnly,
                [.. allUserPublicationRoles]
            );

            foreach (var publicationRole in publicationRolesForTargetUser)
            {
                userPublicationRoleRepository
                    .Setup(r =>
                        r.MarkEmailAsSent(publicationRole.Id, timeProvider.GetUtcNow(), It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. allUserPrereleaseRoles]);

            foreach (var prereleaseRole in prereleaseRolesForTargetUser)
            {
                userPrereleaseRoleRepository
                    .Setup(r =>
                        r.MarkEmailAsSent(prereleaseRole.Id, timeProvider.GetUtcNow(), It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendInviteEmail(inactiveUser.Email, prereleaseRolesInfo, publicationRolesInfo))
                .Returns(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                timeProvider: timeProvider
            );

            await service.NotifyUserOfInvite(userId: inactiveUser.Id);

            MockUtils.VerifyAllMocks(
                userRepository,
                userPublicationRoleRepository,
                userPrereleaseRoleRepository,
                emailTemplateService
            );
        }

        [Fact]
        public async Task UserIsActive_ThrowsArgumentException()
        {
            User activeUser = _dataFixture.DefaultUser();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            var service = BuildService(userRepository: userRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.NotifyUserOfInvite(userId: activeUser.Id)
            );

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task UserDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            userRepository.Setup(r => r.FindUserById(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var service = BuildService(userRepository: userRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfInvite(userId: userId)
            );

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User inactiveUser = _dataFixture.DefaultUserWithPendingInvite();

            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(inactiveUser)
                .WithPublication(_dataFixture.DefaultPublication())
                .GenerateList(3);
            var userPrereleaseRoles = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(inactiveUser)
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .GenerateList(3);

            var prereleaseRolesInfo = userPrereleaseRoles
                .Select(urr => (urr.ReleaseVersion.Release.Publication.Title, urr.ReleaseVersion.Release.Title))
                .ToHashSet();

            var publicationRolesInfo = userPublicationRoles
                .Select(upr => (upr.Publication.Title, upr.Role))
                .ToHashSet();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);
            // Set up the current time in UTC
            var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

            userRepository
                .Setup(r => r.FindUserById(inactiveUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPublicationRoles]);

            foreach (var publicationRole in userPublicationRoles)
            {
                userPublicationRoleRepository
                    .Setup(r =>
                        r.MarkEmailAsSent(publicationRole.Id, timeProvider.GetUtcNow(), It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPrereleaseRoles]);

            foreach (var prereleaseRole in userPrereleaseRoles)
            {
                userPrereleaseRoleRepository
                    .Setup(r =>
                        r.MarkEmailAsSent(prereleaseRole.Id, timeProvider.GetUtcNow(), It.IsAny<CancellationToken>())
                    )
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendInviteEmail(inactiveUser.Email, prereleaseRolesInfo, publicationRolesInfo))
                .Returns(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object,
                timeProvider: timeProvider
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfInvite(userId: inactiveUser.Id)
            );

            MockUtils.VerifyAllMocks(
                userRepository,
                userPublicationRoleRepository,
                userPrereleaseRoleRepository,
                emailTemplateService
            );
        }
    }

    public class NotifyUserOfNewPublicationRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userPublicationRoleRepository
                .Setup(r => r.MarkEmailAsSent(userPublicationRole.Id, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userPublicationRole);

            emailTemplateService
                .Setup(s =>
                    s.SendPublicationRoleEmail(
                        userPublicationRole.User.Email,
                        userPublicationRole.Publication.Title,
                        userPublicationRole.Role
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            await service.NotifyUserOfNewPublicationRole(userPublicationRole.Id);

            MockUtils.VerifyAllMocks(userPublicationRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task RoleDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, []);

            var service = BuildService(userPublicationRoleRepository: userPublicationRoleRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewPublicationRole(Guid.NewGuid())
            );

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication());

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userPublicationRoleRepository
                .Setup(r => r.MarkEmailAsSent(userPublicationRole.Id, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userPublicationRole);

            emailTemplateService
                .Setup(s =>
                    s.SendPublicationRoleEmail(
                        userPublicationRole.User.Email,
                        userPublicationRole.Publication.Title,
                        userPublicationRole.Role
                    )
                )
                .Returns(new BadRequestResult());

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewPublicationRole(userPublicationRole.Id)
            );

            MockUtils.VerifyAllMocks(userPublicationRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewPreReleaseRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success_ActiveUser()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userPrereleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUser()) // Active user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userPrereleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userPrereleaseRole);
            userPrereleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userPrereleaseRole.Id, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userPrereleaseRole.User.Email,
                        userPrereleaseRole.ReleaseVersion.Release.Publication.Title,
                        userPrereleaseRole.ReleaseVersion.Release.Title,
                        false, // Not a new user due to being active
                        userPrereleaseRole.ReleaseVersion.Release.PublicationId,
                        userPrereleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userPrereleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(userPrereleaseRole.Id);

            MockUtils.VerifyAllMocks(preReleaseService, userPrereleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task Success_InactiveUser()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userPrereleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUserWithPendingInvite()) // Inactive user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userPrereleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userPrereleaseRole);
            userPrereleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userPrereleaseRole.Id, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userPrereleaseRole.User.Email,
                        userPrereleaseRole.ReleaseVersion.Release.Publication.Title,
                        userPrereleaseRole.ReleaseVersion.Release.Title,
                        true, // Is new user due to pending invite
                        userPrereleaseRole.ReleaseVersion.Release.PublicationId,
                        userPrereleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userPrereleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(userPrereleaseRole.Id);

            MockUtils.VerifyAllMocks(preReleaseService, userPrereleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userPrereleaseRole = _dataFixture
                .DefaultUserPrereleaseRole()
                .WithUser(_dataFixture.DefaultUser()) // Active user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userPrereleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userPrereleaseRole);
            userPrereleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userPrereleaseRole.Id, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userPrereleaseRole.User.Email,
                        userPrereleaseRole.ReleaseVersion.Release.Publication.Title,
                        userPrereleaseRole.ReleaseVersion.Release.Title,
                        false, // Not a new user due to being active
                        userPrereleaseRole.ReleaseVersion.Release.PublicationId,
                        userPrereleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userPrereleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(new BadRequestResult());

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(userPrereleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(preReleaseService, userPrereleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task RoleDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userPrereleaseRoleRepository = new Mock<IUserPrereleaseRoleRepository>(MockBehavior.Strict);
            userPrereleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, []);

            var service = BuildService(userPrereleaseRoleRepository: userPrereleaseRoleRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(Guid.NewGuid())
            );

            MockUtils.VerifyAllMocks(userPrereleaseRoleRepository);
        }
    }

    private static UserResourceRoleNotificationService BuildService(
        ContentDbContext? contentDbContext = null,
        IPreReleaseService? preReleaseService = null,
        IUserRepository? userRepository = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserPrereleaseRoleRepository? userPrereleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        TimeProvider? timeProvider = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            preReleaseService: preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
            userRepository: userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            emailTemplateService: emailTemplateService ?? Mock.Of<IEmailTemplateService>(MockBehavior.Strict),
            userPrereleaseRoleRepository: userPrereleaseRoleRepository
                ?? Mock.Of<IUserPrereleaseRoleRepository>(MockBehavior.Strict),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            timeProvider ?? new FakeTimeProvider(DateTimeOffset.UtcNow)
        );
    }
}
