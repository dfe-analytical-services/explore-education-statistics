#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
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
using MockQueryable;
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

            User userToCreate = _dataFixture.DefaultUserWithPendingInvite();

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

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<List<UserReleaseRole>>(l =>
                            l.Count == 2
                            && l[0].UserId == userToCreate.Id
                            && l[0].ReleaseVersionId == releaseVersionIds.ElementAt(0)
                            && l[0].Role == ReleaseRole.Contributor
                            && l[0].CreatedById == CreatedById
                            && Math.Abs((l[0].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                            && l[1].UserId == userToCreate.Id
                            && l[1].ReleaseVersionId == releaseVersionIds.ElementAt(1)
                            && l[1].Role == ReleaseRole.Contributor
                            && l[1].CreatedById == CreatedById
                            && Math.Abs((l[1].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
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
                .ReturnsAsync(userToCreate);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewContributorRoles(
                        userToCreate.Id,
                        publication.Title,
                        releaseVersionIds,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userRepository: userRepository.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object
                );

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: releaseVersionIds
                );

                result.AssertRight();
            }

            VerifyAllMocks(
                userReleaseRoleRepository,
                userRepository,
                releaseVersionRepository,
                userResourceRoleNotificationService
            );
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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewContributorRoles(
                        user.Id,
                        publication.Title,
                        newReleaseVersionIds,
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
                        It.Is<List<UserReleaseRole>>(l =>
                            l.Count == 2
                            && l[0].UserId == user.Id
                            && l[0].ReleaseVersionId == newReleaseVersionIds.ElementAt(0)
                            && l[0].Role == ReleaseRole.Contributor
                            && l[0].CreatedById == CreatedById
                            && Math.Abs((l[0].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                            && l[1].UserId == user.Id
                            && l[1].ReleaseVersionId == newReleaseVersionIds.ElementAt(1)
                            && l[1].Role == ReleaseRole.Contributor
                            && l[1].CreatedById == CreatedById
                            && Math.Abs((l[1].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

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
        public async Task InactiveUser_FailsSendingEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            User userToCreate = _dataFixture.DefaultUserWithPendingInvite();

            var releaseVersionIds = SetOf(publication.Releases[0].Versions[0].Id);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(publication.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync([.. releaseVersionIds]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(userToCreate.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            userRepository
                .Setup(mock =>
                    mock.CreateOrUpdate(
                        userToCreate.Email,
                        Role.Analyst,
                        CreatedById,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(userToCreate);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.CreateManyIfNotExists(
                        It.Is<List<UserReleaseRole>>(l =>
                            l.Count == 1
                            && l[0].UserId == userToCreate.Id
                            && l[0].ReleaseVersionId == releaseVersionIds.ElementAt(0)
                            && l[0].Role == ReleaseRole.Contributor
                            && l[0].CreatedById == CreatedById
                            && Math.Abs((l[0].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(Strict);
            userResourceRoleNotificationService
                .Setup(mock =>
                    mock.NotifyUserOfNewContributorRoles(
                        userToCreate.Id,
                        publication.Title,
                        releaseVersionIds,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new EmailSendFailedException(""));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object,
                    userResourceRoleNotificationService: userResourceRoleNotificationService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object
                );

                await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                    await service.InviteContributor(
                        email: userToCreate.Email,
                        publicationId: publication.Id,
                        releaseVersionIds: releaseVersionIds
                    )
                );
            }

            VerifyAllMocks(
                userResourceRoleNotificationService,
                releaseVersionRepository,
                userRepository,
                userReleaseRoleRepository
            );
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
                        It.Is<List<UserReleaseRole>>(l =>
                            l.Count == 1
                            && l[0].UserId == user.Id
                            && l[0].ReleaseVersionId == releaseVersionIds.ElementAt(0)
                            && l[0].Role == ReleaseRole.Contributor
                            && l[0].CreatedById == CreatedById
                            && Math.Abs((l[0].Created - DateTime.UtcNow).Milliseconds)
                                <= AssertExtensions.TimeWithinMillis
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

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
                .Setup(mock => mock.FindActiveUserByEmail(user.Email, It.IsAny<CancellationToken>()))
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
                        email: user.Email,
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
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]);

            User userWithPendingInvite = _dataFixture.DefaultUserWithPendingInvite();

            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithRole(ReleaseRole.Contributor)
                .ForIndex(
                    0,
                    s => s.SetUser(userWithPendingInvite).SetReleaseVersion(publication.Releases[0].Versions[0])
                )
                .ForIndex(
                    1,
                    s => s.SetUser(userWithPendingInvite).SetReleaseVersion(publication.Releases[0].Versions[1])
                )
                .ForIndex(
                    2,
                    s => s.SetUser(userWithPendingInvite).SetReleaseVersion(publication.Releases[0].Versions[2])
                )
                // Different release version - should not be removed
                .ForIndex(
                    3,
                    s =>
                        s.SetUser(userWithPendingInvite)
                            .SetReleaseVersion(
                                _dataFixture
                                    .DefaultReleaseVersion()
                                    .WithRelease(
                                        _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                    )
                            )
                )
                // Different user - should not be removed
                .ForIndex(
                    4,
                    s =>
                        s.SetUser(_dataFixture.DefaultUserWithPendingInvite())
                            .SetReleaseVersion(publication.Releases[0].Versions[0])
                )
                // Different role - should not be removed
                .ForIndex(
                    5,
                    s =>
                        s.SetUser(userWithPendingInvite)
                            .SetReleaseVersion(publication.Releases[0].Versions[0])
                            .SetRole(ReleaseRole.PrereleaseViewer)
                )
                .GenerateList(6);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock =>
                    mock.FindPendingUserInviteByEmail(userWithPendingInvite.Email, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(userWithPendingInvite);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock => mock.Query(ResourceRoleFilter.PendingOnly))
                .Returns(userReleaseRoles.BuildMock());
            userReleaseRoleRepository
                .Setup(mock =>
                    mock.RemoveMany(
                        new[] { userReleaseRoles[0], userReleaseRoles[1], userReleaseRoles[2] },
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    userRepository: userRepository.Object
                );

                var result = await service.RemoveByPublication(
                    email: userWithPendingInvite.Email,
                    publicationId: publication.Id,
                    releaseRole: ReleaseRole.Contributor
                );

                result.AssertRight();
            }

            VerifyAllMocks(userReleaseRoleRepository, userRepository);
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            var service = SetupReleaseInviteService();

            var result = await service.RemoveByPublication(
                email: Email,
                publicationId: Guid.NewGuid(),
                releaseRole: ReleaseRole.Contributor
            );

            result.AssertNotFound();
        }
    }

    private static ReleaseInviteService SetupReleaseInviteService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserRoleService? userRoleService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
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
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(Strict)
        );
    }
}
