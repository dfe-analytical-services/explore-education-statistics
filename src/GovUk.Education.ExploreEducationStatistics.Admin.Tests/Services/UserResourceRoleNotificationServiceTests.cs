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
using MockQueryable;
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
            var releaseRolesForTargetUser = _dataFixture
                .DefaultUserReleaseRole()
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
            var releaseRolesForOtherUser = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUserWithPendingInvite())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .GenerateList(3);

            var allUserPublicationRoles = publicationRolesForTargetUser.Concat(publicationRolesForOtherUser).ToList();
            var allUserReleaseRoles = releaseRolesForTargetUser.Concat(releaseRolesForOtherUser).ToList();

            var releaseRolesInfo = releaseRolesForTargetUser
                .Select(urr =>
                    (urr.ReleaseVersion.Release.Publication.Title, urr.ReleaseVersion.Release.Title, urr.Role)
                )
                .ToHashSet();

            var publicationRolesInfo = publicationRolesForTargetUser
                .Select(upr => (upr.Publication.Title, upr.Role))
                .ToHashSet();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindUserById(inactiveUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            userPublicationRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.PendingOnly))
                .Returns(allUserPublicationRoles.BuildMock());

            foreach (var publicationRole in publicationRolesForTargetUser)
            {
                userPublicationRoleRepository
                    .Setup(r => r.MarkEmailAsSent(publicationRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            userReleaseRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.PendingOnly))
                .Returns(allUserReleaseRoles.BuildMock());

            foreach (var releaseRole in releaseRolesForTargetUser)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(releaseRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendInviteEmail(inactiveUser.Email, releaseRolesInfo, publicationRolesInfo))
                .Returns(Unit.Instance);

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfInvite(userId: inactiveUser.Id);

            MockUtils.VerifyAllMocks(
                userRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
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
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(inactiveUser)
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .GenerateList(3);

            var releaseRolesInfo = userReleaseRoles
                .Select(urr =>
                    (urr.ReleaseVersion.Release.Publication.Title, urr.ReleaseVersion.Release.Title, urr.Role)
                )
                .ToHashSet();

            var publicationRolesInfo = userPublicationRoles
                .Select(upr => (upr.Publication.Title, upr.Role))
                .ToHashSet();

            var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userRepository
                .Setup(r => r.FindUserById(inactiveUser.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveUser);

            userPublicationRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.PendingOnly))
                .Returns(userPublicationRoles.BuildMock());

            foreach (var publicationRole in userPublicationRoles)
            {
                userPublicationRoleRepository
                    .Setup(r => r.MarkEmailAsSent(publicationRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            userReleaseRoleRepository
                .Setup(r => r.Query(ResourceRoleFilter.PendingOnly))
                .Returns(userReleaseRoles.BuildMock());

            foreach (var releaseRole in userReleaseRoles)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(releaseRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendInviteEmail(inactiveUser.Email, releaseRolesInfo, publicationRolesInfo))
                .Returns(new BadRequestResult());

            var service = BuildService(
                userRepository: userRepository.Object,
                emailTemplateService: emailTemplateService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfInvite(userId: inactiveUser.Id)
            );

            MockUtils.VerifyAllMocks(
                userRepository,
                userPublicationRoleRepository,
                userReleaseRoleRepository,
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
                .Setup(r => r.MarkEmailAsSent(userPublicationRole.Id, It.IsAny<CancellationToken>()))
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
                .Setup(r => r.MarkEmailAsSent(userPublicationRole.Id, It.IsAny<CancellationToken>()))
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

    public class NotifyUserOfNewReleaseRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userReleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);

            emailTemplateService
                .Setup(s =>
                    s.SendReleaseRoleEmail(
                        userReleaseRole.User.Email,
                        userReleaseRole.ReleaseVersion.Release.Publication.Title,
                        userReleaseRole.ReleaseVersion.Release.Title,
                        userReleaseRole.ReleaseVersion.Release.PublicationId,
                        userReleaseRole.ReleaseVersionId,
                        userReleaseRole.Role
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewReleaseRole(userReleaseRole.Id);

            MockUtils.VerifyAllMocks(userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task RoleDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, []);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewReleaseRole(Guid.NewGuid())
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                );

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);
            userReleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendReleaseRoleEmail(
                        userReleaseRole.User.Email,
                        userReleaseRole.ReleaseVersion.Release.Publication.Title,
                        userReleaseRole.ReleaseVersion.Release.Title,
                        userReleaseRole.ReleaseVersion.Release.PublicationId,
                        userReleaseRole.ReleaseVersionId,
                        userReleaseRole.Role
                    )
                )
                .Returns(new BadRequestResult());

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewReleaseRole(userReleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewContributorRolesTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            var (release1, release2) = _dataFixture.DefaultRelease().WithPublication(publication).GenerateTuple2();

            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithRole(ReleaseRole.Contributor)
                .ForIndex(0, s => s.SetReleaseVersion(_dataFixture.DefaultReleaseVersion().WithRelease(release1)))
                .ForIndex(1, s => s.SetReleaseVersion(_dataFixture.DefaultReleaseVersion().WithRelease(release2)))
                // Create a duplicate release across two roles, to test that the releases info passed to the email contains all distinct releases
                .ForIndex(2, s => s.SetReleaseVersion(_dataFixture.DefaultReleaseVersion().WithRelease(release1)))
                .GenerateList(3);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. userReleaseRoles]);

            foreach (var userReleaseRole in userReleaseRoles)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            var releasesInfo = userReleaseRoles
                .Select(urr => urr.ReleaseVersion.Release)
                .Distinct()
                .Select(r => (r.Year, r.TimePeriodCoverage, r.Title))
                .ToHashSet();

            var userReleaseRoleIds = userReleaseRoles.Select(r => r.Id).ToHashSet();

            emailTemplateService
                .Setup(s => s.SendContributorInviteEmail(user.Email, publication.Title, releasesInfo))
                .Returns(Unit.Instance);

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds);

            MockUtils.VerifyAllMocks(userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task EmptyRoleIds_ThrowsArgumentException()
        {
            var service = BuildService();

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.NotifyUserOfNewContributorRoles([]));
        }

        [Fact]
        public async Task SomeRolesDontExist_ThrowsKeyNotFoundException()
        {
            var existingUserReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor)
                .GenerateList(3);

            var userReleaseRoleIds = existingUserReleaseRoles
                .Select(r => r.Id)
                .ToHashSet()
                .Concat([Guid.NewGuid(), Guid.NewGuid()]) // Non-existent role IDs
                .ToHashSet();

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. existingUserReleaseRoles]);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task NotAllRolesAreForTheSameUser_ThrowsArgumentException()
        {
            var user1 = _dataFixture.DefaultUser();
            var user2 = _dataFixture.DefaultUser();

            var existingUserReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor)
                .ForIndex(0, s => s.SetUser(user1))
                .ForIndex(1, s => s.SetUser(user1))
                .ForIndex(2, s => s.SetUser(user2))
                .GenerateList(3);

            var userReleaseRoleIds = existingUserReleaseRoles.Select(r => r.Id).ToHashSet();

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. existingUserReleaseRoles]);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task NotAllRolesAreForTheSamePublication_ThrowsArgumentException()
        {
            Publication publication1 = _dataFixture.DefaultPublication();
            Publication publication2 = _dataFixture.DefaultPublication();

            var existingUserReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithRole(ReleaseRole.Contributor)
                .ForIndex(
                    0,
                    s =>
                        s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication1))
                        )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication1))
                        )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication2))
                        )
                )
                .GenerateList(3);

            var userReleaseRoleIds = existingUserReleaseRoles.Select(r => r.Id).ToHashSet();

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. existingUserReleaseRoles]);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task NotAllRolesAreContributorRoles_ThrowsArgumentException()
        {
            var existingUserReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(2, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(3);

            var userReleaseRoleIds = existingUserReleaseRoles.Select(r => r.Id).ToHashSet();

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. existingUserReleaseRoles]);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();

            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                )
                .WithRole(ReleaseRole.Contributor)
                .GenerateList(3);

            var releasesInfo = userReleaseRoles
                .Select(urr => urr.ReleaseVersion.Release)
                .Distinct()
                .Select(r => (r.Year, r.TimePeriodCoverage, r.Title))
                .ToHashSet();

            var userReleaseRoleIds = userReleaseRoles.Select(r => r.Id).ToHashSet();

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, [.. userReleaseRoles]);

            foreach (var userReleaseRole in userReleaseRoles)
            {
                userReleaseRoleRepository
                    .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            emailTemplateService
                .Setup(s => s.SendContributorInviteEmail(user.Email, publication.Title, releasesInfo))
                .Returns(new BadRequestResult());

            var service = BuildService(
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewContributorRoles(userReleaseRoleIds)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository, emailTemplateService);
        }
    }

    public class NotifyUserOfNewPreReleaseRoleTests : UserResourceRoleNotificationServiceTests
    {
        [Fact]
        public async Task Success_ActiveUser()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser()) // Active user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.PrereleaseViewer);

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userReleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);
            userReleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userReleaseRole.User.Email,
                        userReleaseRole.ReleaseVersion.Release.Publication.Title,
                        userReleaseRole.ReleaseVersion.Release.Title,
                        false, // Not a new user due to being active
                        userReleaseRole.ReleaseVersion.Release.PublicationId,
                        userReleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userReleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(userReleaseRole.Id);

            MockUtils.VerifyAllMocks(preReleaseService, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task Success_InactiveUser()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUserWithPendingInvite()) // Inactive user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.PrereleaseViewer);

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userReleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);
            userReleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userReleaseRole.User.Email,
                        userReleaseRole.ReleaseVersion.Release.Publication.Title,
                        userReleaseRole.ReleaseVersion.Release.Title,
                        true, // Is new user due to pending invite
                        userReleaseRole.ReleaseVersion.Release.PublicationId,
                        userReleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userReleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(Unit.Instance);

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await service.NotifyUserOfNewPreReleaseRole(userReleaseRole.Id);

            MockUtils.VerifyAllMocks(preReleaseService, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task SendingEmailFails_ThrowsEmailSendFailedException()
        {
            var scheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser()) // Active user
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithPublishScheduled(scheduledPublishDate)
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.PrereleaseViewer);

            var preReleaseWindow = new PreReleaseWindow
            {
                Start = DateTimeOffset.UtcNow.AddDays(1),
                ScheduledPublishDate = scheduledPublishDate,
            };

            var preReleaseService = new Mock<IPreReleaseService>(MockBehavior.Strict);
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            preReleaseService
                .Setup(s => s.GetPreReleaseWindow(userReleaseRole.ReleaseVersion))
                .Returns(preReleaseWindow);

            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);
            userReleaseRoleRepository
                .Setup(r => r.MarkEmailAsSent(userReleaseRole.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            emailTemplateService
                .Setup(s =>
                    s.SendPreReleaseInviteEmail(
                        userReleaseRole.User.Email,
                        userReleaseRole.ReleaseVersion.Release.Publication.Title,
                        userReleaseRole.ReleaseVersion.Release.Title,
                        false, // Not a new user due to being active
                        userReleaseRole.ReleaseVersion.Release.PublicationId,
                        userReleaseRole.ReleaseVersionId,
                        preReleaseWindow.Start,
                        userReleaseRole.ReleaseVersion.PublishScheduled!.Value
                    )
                )
                .Returns(new BadRequestResult());

            var service = BuildService(
                preReleaseService: preReleaseService.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(userReleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(preReleaseService, userReleaseRoleRepository, emailTemplateService);
        }

        [Fact]
        public async Task RoleDoesNotExist_ThrowsKeyNotFoundException()
        {
            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, []);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(Guid.NewGuid())
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }

        [Theory]
        [InlineData(ReleaseRole.Approver)]
        [InlineData(ReleaseRole.Contributor)]
        public async Task RoleIsNotAPrereleaseRole_ThrowsKeyNotFoundException(ReleaseRole role)
        {
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(role);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(MockBehavior.Strict);
            userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.AllButExpired, userReleaseRole);

            var service = BuildService(userReleaseRoleRepository: userReleaseRoleRepository.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.NotifyUserOfNewPreReleaseRole(userReleaseRole.Id)
            );

            MockUtils.VerifyAllMocks(userReleaseRoleRepository);
        }
    }

    private static UserResourceRoleNotificationService BuildService(
        ContentDbContext? contentDbContext = null,
        IPreReleaseService? preReleaseService = null,
        IUserRepository? userRepository = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();

        return new(
            contentDbContext: contentDbContext,
            preReleaseService: preReleaseService ?? Mock.Of<IPreReleaseService>(MockBehavior.Strict),
            userRepository: userRepository ?? Mock.Of<IUserRepository>(MockBehavior.Strict),
            emailTemplateService: emailTemplateService ?? Mock.Of<IEmailTemplateService>(MockBehavior.Strict),
            userReleaseRoleRepository: userReleaseRoleRepository
                ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict)
        );
    }
}
