#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserResourceRoleNotificationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class NotifyUserOfNewPublicationRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User activeUser = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            const PublicationRole role = PublicationRole.Allower;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userPublicationRoleRepository
                .Setup(r => r.MarkEmailAsSent(activeUser.Id, publication.Id, role, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendPublicationRoleEmail(activeUser.Email, publication, role))
                .Returns(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            await service.NotifyUserOfNewPublicationRole(userId: activeUser.Id, publication: publication, role: role);

            MockUtils.VerifyAllMocks(userRepository, userPublicationRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task ActiveUserDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = BuildService(userRepository: userRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewPublicationRole(
                    userId: userId,
                    publication: _dataFixture.DefaultPublication(),
                    role: PublicationRole.Allower
                )
            );

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User activeUser = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            const PublicationRole role = PublicationRole.Allower;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userPublicationRoleRepository
                .Setup(r => r.MarkEmailAsSent(activeUser.Id, publication.Id, role, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendPublicationRoleEmail(activeUser.Email, publication, role))
                .Returns(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewPublicationRole(
                    userId: activeUser.Id,
                    publication: publication,
                    role: role
                )
            );

            MockUtils.VerifyAllMocks(userRepository, userPublicationRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewReleaseRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User activeUser = _dataFixture.DefaultUser();
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();
            const ReleaseRole role = ReleaseRole.Contributor;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userReleaseRoleRepository
                .Setup(r =>
                    r.MarkEmailAsSent(activeUser.Id, releaseVersion.Id, role, null, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendReleaseRoleEmail(activeUser.Email, releaseVersion, role))
                .Returns(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewReleaseRole(userId: activeUser.Id, releaseVersion: releaseVersion, role: role);

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task ActiveUserDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = BuildService(userRepository: userRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewReleaseRole(
                    userId: userId,
                    releaseVersion: _dataFixture.DefaultReleaseVersion(),
                    role: ReleaseRole.Contributor
                )
            );

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User activeUser = _dataFixture.DefaultUser();
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();
            const ReleaseRole role = ReleaseRole.Contributor;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserById(activeUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userReleaseRoleRepository
                .Setup(r =>
                    r.MarkEmailAsSent(activeUser.Id, releaseVersion.Id, role, null, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendReleaseRoleEmail(activeUser.Email, releaseVersion, role))
                .Returns(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewReleaseRole(
                    userId: activeUser.Id,
                    releaseVersion: releaseVersion,
                    role: role
                )
            );

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewContributorRolesTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();
            var publicationTitle = "publication-title";
            var releaseVersionIds = CollectionUtils.SetOf(Guid.NewGuid(), Guid.NewGuid());
            const ReleaseRole role = ReleaseRole.Contributor;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository.Setup(r => r.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            foreach (var releaseVersionId in releaseVersionIds)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(user.Id, releaseVersionId, role, null, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendContributorInviteEmail(user.Email, publicationTitle, releaseVersionIds))
                .ReturnsAsync(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewContributorRoles(
                userId: user.Id,
                publicationTitle: publicationTitle,
                releaseVersionIds: releaseVersionIds
            );

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task UserDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            userRepository.Setup(r => r.FindUserById(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            var service = BuildService(userRepository: userRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewContributorRoles(
                    userId: userId,
                    publicationTitle: "publication-title",
                    releaseVersionIds: []
                )
            );

            MockUtils.VerifyAllMocks(userRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User user = _dataFixture.DefaultUser();
            var publicationTitle = "publication-title";
            var releaseVersionIds = CollectionUtils.SetOf(Guid.NewGuid(), Guid.NewGuid());
            const ReleaseRole role = ReleaseRole.Contributor;

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository.Setup(r => r.FindUserById(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            foreach (var releaseVersionId in releaseVersionIds)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(user.Id, releaseVersionId, role, null, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendContributorInviteEmail(user.Email, publicationTitle, releaseVersionIds))
                .ReturnsAsync(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewContributorRoles(
                    userId: user.Id,
                    publicationTitle: publicationTitle,
                    releaseVersionIds: releaseVersionIds
                )
            );

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewPreReleaseRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success_ActiveUser()
        {
            User activeUser = _dataFixture.DefaultUser();
            var releaseVersionId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserByEmail(activeUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userReleaseRoleRepository
                .Setup(r =>
                    r.MarkEmailAsSent(
                        activeUser.Id,
                        releaseVersionId,
                        ReleaseRole.PrereleaseViewer,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendPreReleaseInviteEmail(activeUser.Email, releaseVersionId, false))
                .ReturnsAsync(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(
                userEmail: activeUser.Email,
                releaseVersionId: releaseVersionId
            );

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task Success_InactiveUser()
        {
            User inactiveUser = _dataFixture.DefaultUserWithPendingInvite();
            var releaseVersionId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserByEmail(inactiveUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            emailTemplateService
                .Setup(s => s.SendPreReleaseInviteEmail(inactiveUser.Email, releaseVersionId, true))
                .ReturnsAsync(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(
                userEmail: inactiveUser.Email,
                releaseVersionId: releaseVersionId
            );

            MockUtils.VerifyAllMocks(userRepository, emailTemplateService);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User activeUser = _dataFixture.DefaultUser();
            var releaseVersionId = Guid.NewGuid();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindActiveUserByEmail(activeUser.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeUser);

            userReleaseRoleRepository
                .Setup(r =>
                    r.MarkEmailAsSent(
                        activeUser.Id,
                        releaseVersionId,
                        ReleaseRole.PrereleaseViewer,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s => s.SendPreReleaseInviteEmail(activeUser.Email, releaseVersionId, false))
                .ReturnsAsync(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(
                    userEmail: activeUser.Email,
                    releaseVersionId: releaseVersionId
                )
            );

            MockUtils.VerifyAllMocks(userRepository, userReleaseRoleRepository, emailTemplateService);
        }
    }

    private static UserResourceRoleNotificationService BuildService(
        ContentDbContext? contentDbContext = null,
        IUserRepository? userRepository = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            userRepository: userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            emailTemplateService: emailTemplateService ?? Mock.Of<IEmailTemplateService>(MockBehavior.Strict),
            userReleaseRoleRepository: userReleaseRoleRepository
                ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict)
        );
    }
}
