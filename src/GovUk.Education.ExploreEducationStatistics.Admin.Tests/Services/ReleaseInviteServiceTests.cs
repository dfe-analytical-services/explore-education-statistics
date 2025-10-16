#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
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
using Microsoft.Extensions.Options;
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
    private const string NotifyContributorTemplateId = "contributor-invite-template-id";

    private readonly DataFixture _dataFixture = new();

    public class InviteContributorTests : ReleaseInviteServiceTests
    {
        [Fact]
        public async Task InactiveUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var releaseVersionIds = ListOf(
                publication.Releases[0].Versions[0].Id,
                publication.Releases[1].Versions[0].Id);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01\n* Academic year 2001/02");
            emailService.Setup(mock => mock.SendEmail(
                Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.CreateManyIfNotExists(
                    releaseVersionIds,
                    Email,
                    ReleaseRole.Contributor,
                    true,
                    CreatedById,
                    null)
                )
                .Returns(Task.CompletedTask);

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(releaseVersionIds);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.CreateOrUpdate(
                    Email,
                    Role.Analyst,
                    CreatedById,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dataFixture.DefaultUser());
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object,
                    userRepository: userRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: releaseVersionIds);

                result.AssertRight();
            }

            emailService.Verify(
                s => s.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ), Times.Once
            );

            VerifyAllMocks(
                emailService,
                userReleaseInviteRepository,
                userRepository,
                releaseVersionRepository);
        }

        [Fact]
        public async Task ActiveUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var user = _dataFixture.DefaultUser()
                .WithEmail(Email)
                .Generate();

            var newReleaseVersionIds = ListOf(
                publication.Releases[0].Versions[0].Id,
                publication.Releases[1].Versions[0].Id);

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
                .ToList();

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.UserReleaseRoles.Add(existingUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2001/02");
            emailService.Setup(mock => mock.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var userRoleService = new Mock<IUserRoleService>(Strict);
            userRoleService
                .Setup(mock =>
                    mock.GetAssociatedGlobalRoleNameForReleaseRole(ReleaseRole.Contributor))
                .Returns(RoleNames.Analyst);
            userRoleService
                .Setup(mock =>
                    mock.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, user.Id))
                .ReturnsAsync(Unit.Instance);

            var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
            userReleaseRoleRepository
                .Setup(mock => mock.CreateManyIfNotExists(
                    user.Id,
                    missingReleaseRoleReleaseVersionIds,
                    ReleaseRole.Contributor,
                    CreatedById))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newReleaseVersionIds);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRoleService: userRoleService.Object,
                    emailService: emailService.Object,
                    userReleaseRoleRepository: userReleaseRoleRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: newReleaseVersionIds);

                result.AssertRight();
            }

            emailService.Verify(
                s => s.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ), Times.Once
            );

            VerifyAllMocks(
                userRoleService,
                emailService,
                userReleaseRoleRepository,
                releaseVersionRepository,
                userRepository);
        }

        [Fact]
        public async Task ActiveUserAlreadyHasReleaseRoles() 
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var user = _dataFixture.DefaultUser()
                .WithEmail(Email)
                .Generate();

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
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication.Id, 
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([
                    publication.Releases[0].Versions[0].Id, 
                    publication.Releases[1].Versions[0].Id]);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRepository: userRepository.Object,
                    releaseVersionRepository: releaseVersionRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.Releases[0].Versions[0].Id,
                        publication.Releases[1].Versions[0].Id));

                result.AssertBadRequest(ValidationErrorMessages.UserAlreadyHasReleaseRoles);
            }

            VerifyAllMocks(
                userRepository,
                releaseVersionRepository);
        }

        [Fact]
        public async Task InactiveUser_FailsSendingEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var releaseVersionIds = ListOf(publication.Releases[0].Versions[0].Id);

            var contentDbContextId = Guid.NewGuid().ToString();
            var usersAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01");
            emailService.Setup(mock => mock.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(releaseVersionIds);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: releaseVersionIds);

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestResult>(actionResult);
            }

            emailService.Verify(
                s => s.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ), Times.Once
            );

            VerifyAllMocks(
                emailService,
                releaseVersionRepository,
                userRepository);
        }

        [Fact]
        public async Task ActiveExistingUser_FailsSendingEmail()
        {
            var user = _dataFixture.DefaultUser()
                .WithEmail(Email)
                .Generate();

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01");
            emailService.Setup(mock => mock.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([publication.Releases[0].Versions[0].Id]);

            var userRepository = new Mock<IUserRepository>(Strict);
            userRepository
                .Setup(mock => mock.FindActiveUserByEmail(Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    userRepository: userRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.Releases[0].Versions[0].Id));

                emailService.Verify(
                    s => s.SendEmail(
                        Email,
                        NotifyContributorTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(
                    emailService,
                    releaseVersionRepository,
                    userRepository);

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestResult>(actionResult);
            }
        }

        [Fact]
        public async Task NotAllReleasesBelongToPublication()
        {
            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1))
                .GenerateTuple2();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);
            releaseVersionRepository
                .Setup(mock => mock.ListLatestReleaseVersionIds(
                    publication1.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([publication1.Releases[0].Versions[0].Id]);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object);

                var result = await service.InviteContributor(
                    email: Email,
                    publicationId: publication1.Id,
                    releaseVersionIds: ListOf(publication1.Releases[0].Versions[0].Id,
                        publication2.Releases[0].Versions[0].Id));

                result.AssertBadRequest(ValidationErrorMessages.NotAllReleasesBelongToPublication);
            }

            VerifyAllMocks(releaseVersionRepository);
        }

        private static Dictionary<string, dynamic> GetExpectedContributorInviteTemplateValues(
            string publicationTitle,
            string releaseList)
        {
            return new()
            {
                { "url", "https://localhost" },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };
        }
    }

    public class RemoveByPublicationTests : ReleaseInviteServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var publication = _dataFixture.DefaultPublication()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
            userReleaseInviteRepository
                .Setup(mock => mock.RemoveByPublicationAndEmail(
                    publication.Id,
                    Email,
                    default,
                    ReleaseRole.Contributor))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    userReleaseInviteRepository: userReleaseInviteRepository.Object);

                var result = await service.RemoveByPublication(
                    email: Email,
                    publicationId: publication.Id,
                    releaseRole: ReleaseRole.Contributor);

                result.AssertRight();
            }

            VerifyAllMocks(userReleaseInviteRepository);
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            UserReleaseInvite userReleaseInvite = _dataFixture.DefaultUserReleaseInvite()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication())))
            .WithRole(ReleaseRole.Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: userReleaseInvite.Email,
                    publicationId: Guid.NewGuid(),
                    releaseRole: userReleaseInvite.Role);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites
                    .ToListAsync();
                Assert.Single(actualUserReleaseInvites);

                Assert.Equal(userReleaseInvite.Id, actualUserReleaseInvites[0].Id);
            }
        }
    }

    private static IOptions<AppOptions> DefaultAppOptions()
    {
        return new AppOptions { Url = "https://localhost" }.ToOptionsWrapper();
    }

    private static IOptions<NotifyOptions> DefaultNotifyOptions()
    {
        return new NotifyOptions { ContributorTemplateId = NotifyContributorTemplateId }.ToOptionsWrapper();
    }

    private static ReleaseInviteService SetupReleaseInviteService(
        ContentDbContext? contentDbContext = null,
        UsersAndRolesDbContext? usersAndRolesDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserRepository? userRepository = null,
        IUserService? userService = null,
        IUserRoleService? userRoleService = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new ReleaseInviteService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            userRepository ?? Mock.Of<IUserRepository>(Strict),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict),
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions()
        );
    }
}
