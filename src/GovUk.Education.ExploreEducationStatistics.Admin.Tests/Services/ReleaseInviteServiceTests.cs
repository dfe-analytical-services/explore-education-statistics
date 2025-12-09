#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
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
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class ReleaseInviteServiceTests
{
    private static readonly string Email = "test@test.com";
    private static readonly Guid CreatedById = Guid.NewGuid();

    private readonly DataFixture _dataFixture = new();

    public class InviteContributorTests : ReleaseInviteServiceTests
    {
        [Fact]
        public async Task InactiveUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(2));

            var releaseVersionIds = SetOf(
                publication.Releases[0].Versions[0].Id,
                publication.Releases[1].Versions[0].Id
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
            emailTemplateService
                .Setup(mock => mock.SendContributorInviteEmail(Email, publication.Title, releaseVersionIds))
                .ReturnsAsync(Unit.Instance);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        releaseVersionIds.ToList(),
                        Email,
                        ReleaseRole.Contributor,
                        true,
                        CreatedById,
                        null
                    )
                )
                .Returns(Task.CompletedTask);

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. releaseVersionIds]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(Email, Role.Analyst, CreatedById, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(_dataFixture.DefaultUser());
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: releaseVersionIds
                );

                result.AssertRight();
            }

            VerifyAllMocks(emailTemplateService, userReleaseInviteRepository, userRepository, releaseVersionRepository);
        }

        [Fact]
        public async Task ActiveUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(2));

            User user = _dataFixture.DefaultUser().WithEmail(Email);

            var newReleaseVersionIds = SetOf(
                publication.Releases[0].Versions[0].Id,
                publication.Releases[1].Versions[0].Id
            );

            var existingUserReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.Releases[0].Versions[0],
                Role = ReleaseRole.Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var missingReleaseRoleReleaseVersionIds = newReleaseVersionIds
                .Except([existingUserReleaseRole.ReleaseVersion.Id])
                .ToHashSet();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.UserReleaseRoles.Add(existingUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewContributorRoles(
                        user.Id,
                        publication.Title,
                        missingReleaseRoleReleaseVersionIds,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userRoleService = new Mock<IUserRoleService>(Strict);
            userRoleService
                .Setup(mock => mock.GetAssociatedGlobalRoleNameForReleaseRole(ReleaseRole.Contributor))
                .Returns(RoleNames.Analyst);
            userRoleService
                .Setup(mock => mock.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, user.Id))
                .ReturnsAsync(Unit.Instance);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        user.Id,
                        missingReleaseRoleReleaseVersionIds.ToList(),
                        ReleaseRole.Contributor,
                        CreatedById
                    )
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. newReleaseVersionIds]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userRoleService: userRoleService.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: newReleaseVersionIds
                );

                result.AssertRight();
            }

            VerifyAllMocks(
                userRoleService,
                userResourceRoleNotificationService,
                userReleaseRoleRepository,
                releaseVersionRepository,
                userRepository
            );
        }

        [Fact]
        public async Task ActiveUserAlreadyHasReleaseRoles()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(2));

            User user = _dataFixture.DefaultUser().WithEmail(Email);

            var userRelease1Role = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.Releases[0].Versions[0],
                Role = ReleaseRole.Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var userRelease2Role = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.Releases[1].Versions[0],
                Role = ReleaseRole.Contributor,
                Created = new DateTime(2001, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.UserReleaseRoles.AddRange(userRelease1Role, userRelease2Role);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([publication.Releases[0].Versions[0].Id, publication.Releases[1].Versions[0].Id]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: SetOf(
                        publication.Releases[0].Versions[0].Id,
                        publication.Releases[1].Versions[0].Id
                    )
                );

                result.AssertBadRequest(ValidationErrorMessages.UserAlreadyHasReleaseRoles);
            }

            VerifyAllMocks(userRepository, releaseVersionRepository);
        }

        [Fact]
        public async Task InactiveUser_FailsSendingEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            var releaseVersionIds = SetOf(publication.Releases[0].Versions[0].Id);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailTemplateService = new Mock<IEmailTemplateService>(Strict);
            emailTemplateService
                .Setup(mock => mock.SendContributorInviteEmail(Email, publication.Title, releaseVersionIds))
                .ReturnsAsync(new BadRequestResult());

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. releaseVersionIds]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    emailTemplateService: emailTemplateService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: releaseVersionIds
                );

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestResult>(actionResult);
            }

            VerifyAllMocks(emailTemplateService, releaseVersionRepository, userRepository);
        }

        [Fact]
        public async Task ActiveExistingUser_FailsSendingEmail()
        {
            User user = _dataFixture.DefaultUser().WithEmail(Email);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            var releaseVersionIds = SetOf(publication.Releases[0].Versions[0].Id);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        user.Id,
                        releaseVersionIds.ToList(),
                        ReleaseRole.Contributor,
                        CreatedById
                    )
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewContributorRoles(
                        user.Id,
                        publication.Title,
                        releaseVersionIds,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new EmailSendFailedException(""));

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. releaseVersionIds]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                    await service.InviteContributor(
                        email: Email,
                        publicationId: publication.Id,
                        releaseVersionIds: releaseVersionIds
                    )
                );

                VerifyAllMocks(
                    userResourceRoleNotificationService,
                    releaseVersionRepository,
                    userRepository,
                    userReleaseRoleRepository
                );
            }
        }

        [Fact]
        public async Task NotAllReleasesBelongToPublication()
        {
            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1))
                .GenerateTuple2();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication1.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([publication1.Releases[0].Versions[0].Id]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication1.Id,
                    releaseVersionIds: SetOf(
                        publication1.Releases[0].Versions[0].Id,
                        publication2.Releases[0].Versions[0].Id
                    )
                );

                result.AssertBadRequest(ValidationErrorMessages.NotAllReleasesBelongToPublication);
            }

            VerifyAllMocks(releaseVersionRepository);
        }
    }

    public class RemoveByPublicationTests : ReleaseInviteServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var publication = _dataFixture.DefaultPublication().Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock =>
                    mock.RemoveByPublicationAndEmail(publication.Id, Email, default, ReleaseRole.Contributor)
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object
                );

                var result = await service.RemoveByPublication(
                    email: Email,
                    publicationId: publication.Id,
                    releaseRole: ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userReleaseInviteRepository);
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            UserReleaseInvite userReleaseInvite = _dataFixture
                .DefaultUserReleaseInvite()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
                )
                .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: userReleaseInvite.Email,
                    publicationId: Guid.NewGuid(),
                    releaseRole: userReleaseInvite.Role
                );

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites.ToListAsync();
                Assert.Single(actualUserReleaseInvites);

                Assert.Equal(userReleaseInvite.Id, actualUserReleaseInvites[0].Id);
            }
        }
    }

    private static ReleaseInviteService SetupReleaseInviteService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserRoleService? userRoleService = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IEmailTemplateService? emailTemplateService = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new ReleaseInviteService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            emailTemplateService: emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
            userResourceRoleNotificationService: userResourceRoleNotificationService
                ?? Mock.Of<IUserResourceRoleNotificationService>(Strict)
        );
    }
}
