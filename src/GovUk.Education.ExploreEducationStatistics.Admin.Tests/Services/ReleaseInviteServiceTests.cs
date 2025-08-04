#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseInviteServiceTests
{
    private static readonly string Email = "test@test.com";
    private static readonly Guid CreatedById = Guid.NewGuid();
    private const string NotifyContributorTemplateId = "contributor-invite-template-id";

    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task InviteContributor_CreateInvite()
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
            .Setup(mock => mock.UserHasInvites(
                releaseVersionIds,
                Email,
                ReleaseRole.Contributor))
            .ReturnsAsync(false)
            .Verifiable();
        userReleaseInviteRepository
            .Setup(mock => mock.CreateManyIfNotExists(
                releaseVersionIds,
                Email,
                ReleaseRole.Contributor,
                true,
                CreatedById,
                null)
            )
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailService: emailService.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object);

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
            userReleaseInviteRepository);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
            .ToListAsync();

            Assert.Single(userInvites);

            Assert.Equal(Email, userInvites[0].Email);
            Assert.Equal(Role.Analyst.GetEnumValue(), userInvites[0].RoleId);
            Assert.Equal(CreatedById.ToString(), userInvites[0].CreatedById);
            Assert.False(userInvites[0].Accepted);
            Assert.InRange(DateTime.UtcNow.Subtract(userInvites[0].Created).Milliseconds, 0, 1500);
        }
    }

    [Fact]
    public async Task InviteContributor_ExistingUser()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(2));

        var user = new User
        {
            Email = Email,
        };

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
            contentDbContext.Users.Add(user);
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

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>(Strict);
        userReleaseRoleAndInviteManager
            .Setup(mock => mock.CreateManyIfNotExists(
                user.Id,
                missingReleaseRoleReleaseVersionIds,
                ReleaseRole.Contributor,
                CreatedById))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userRoleService: userRoleService.Object,
                emailService: emailService.Object,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object);

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
            userReleaseRoleAndInviteManager);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
            .AsQueryable()
            .ToListAsync();

            Assert.Empty(userInvites); // user already exists, so don't create a user invite

            var userReleaseInvites = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userReleaseInvites); // no release invite created as user already exists
        }
    }

    [Fact]
    public async Task InviteContributor_UserAlreadyHasReleaseRoleInvites()
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

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvites(
                releaseVersionIds,
                Email,
                ReleaseRole.Contributor))
            .ReturnsAsync(true)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                userReleaseInviteRepository: userReleaseInviteRepository.Object);

            var result = await service.InviteContributor(
                email: Email,
                publicationId: publication.Id,
                releaseVersionIds: releaseVersionIds);

            result.AssertBadRequest(UserAlreadyHasReleaseRoleInvites);
        }

        VerifyAllMocks(userReleaseInviteRepository);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userInvites);

            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Empty(userReleaseRoles);
        }
    }

    [Fact]
    public async Task InviteContributor_UserAlreadyHasReleaseRoles()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(2));

        var user = new User
        {
            Email = Email,
        };

        var userRelease1Role = new UserReleaseRole
        {
            User = user,
            ReleaseVersion = publication.ReleaseVersions[0],
            Role = ReleaseRole.Contributor,
            Created = new DateTime(2000, 1, 1),
            CreatedById = Guid.NewGuid(),
        };

        var userRelease2Role = new UserReleaseRole
        {
            User = user,
            ReleaseVersion = publication.ReleaseVersions[1],
            Role = ReleaseRole.Contributor,
            Created = new DateTime(2001, 1, 1),
            CreatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.Users.Add(user);
            contentDbContext.UserReleaseRoles.AddRange(userRelease1Role, userRelease2Role);
            await contentDbContext.SaveChangesAsync();
        }

        var usersAndRolesDbContextId = Guid.NewGuid().ToString();
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext);

            var result = await service.InviteContributor(
                email: Email,
                publicationId: publication.Id,
                releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id,
                    publication.ReleaseVersions[1].Id));

            result.AssertBadRequest(UserAlreadyHasReleaseRoles);
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userInvites); // user already exists, so don't create a user invite
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Equal(2, userReleaseRoles.Count);
            Assert.Equal(userRelease1Role.Id, userReleaseRoles[0].Id);
            Assert.Equal(userRelease2Role.Id, userReleaseRoles[1].Id);

            var userReleaseInvites = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userReleaseInvites); // only create release invite for missing UserReleaseRole
        }
    }

    [Fact]
    public async Task InviteContributor_NewUser_FailsSendingEmail()
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

        var userReleaseInviteRepository = new Mock<IUserReleaseInviteRepository>(Strict);
        userReleaseInviteRepository
            .Setup(mock => mock.UserHasInvites(
                releaseVersionIds,
                Email,
                ReleaseRole.Contributor))
            .ReturnsAsync(false)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailService: emailService.Object,
                userReleaseInviteRepository: userReleaseInviteRepository.Object);

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
            userReleaseInviteRepository);

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userInvites);
        }
    }

    [Fact]
    public async Task InviteContributor_ExistingUser_FailsSendingEmail()
    {
        var user = new User
        {
            Email = Email,
        };

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.Users.Add(user);
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

        var usersAndRolesDbContextId = Guid.NewGuid().ToString();
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext,
                emailService: emailService.Object);

            var result = await service.InviteContributor(
                email: Email,
                publicationId: publication.Id,
                releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id));

            emailService.Verify(
                s => s.SendEmail(
                    Email,
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);

            var actionResult = result.AssertLeft();
            Assert.IsType<BadRequestResult>(actionResult);
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userInvites); // user already exists, so don't create a user invite
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Empty(userReleaseRoles);

            var userReleaseInvites = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .ToListAsync();
            Assert.Empty(userReleaseInvites);
        }
    }

    [Fact]
    public async Task InviteContributor_NotAllReleasesBelongToPublication()
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

        var usersAndRolesDbContextId = Guid.NewGuid().ToString();
        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleaseInviteService(
                contentDbContext: contentDbContext,
                usersAndRolesDbContext: usersAndRolesDbContext);

            var result = await service.InviteContributor(
                email: Email,
                publicationId: publication1.Id,
                releaseVersionIds: ListOf(publication1.ReleaseVersions[0].Id,
                    publication2.ReleaseVersions[0].Id));

            result.AssertBadRequest(NotAllReleasesBelongToPublication);
        }

        await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
        {
            var userInvites = await usersAndRolesDbContext.UserInvites
                .AsQueryable()
                .ToListAsync();

            Assert.Empty(userInvites);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userReleaseRoles = await contentDbContext.UserReleaseRoles
                .AsQueryable()
                .ToListAsync();
            Assert.Empty(userReleaseRoles);

            var userReleaseInvites = await contentDbContext.UserReleaseInvites
                .AsQueryable()
                .ToListAsync();
            Assert.Empty(userReleaseInvites);
        }
    }

    [Fact]
    public async Task RemoveByPublication()
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
            .Setup(mock => mock.RemoveByPublication(
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
    public async Task RemoveByPublication_NoPublication()
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

    private static Dictionary<string, dynamic> GetExpectedContributorInviteTemplateValues(string publicationTitle,
        string releaseList)
    {
        return new()
        {
            { "url", "https://localhost" },
            { "publication name", publicationTitle },
            { "release list", releaseList },
        };
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
        IUserInviteRepository? userInviteRepository = null,
        IUserReleaseInviteRepository? userReleaseInviteRepository = null,
        IUserReleaseRoleAndInviteManager? userReleaseRoleAndInviteManager = null,
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null)
    {
        contentDbContext ??= InMemoryApplicationDbContext();
        usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

        return new ReleaseInviteService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            userRepository ?? new UserRepository(contentDbContext),
            userService ?? AlwaysTrueUserService(CreatedById).Object,
            userRoleService ?? Mock.Of<IUserRoleService>(Strict),
            userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
            userReleaseInviteRepository ?? Mock.Of<IUserReleaseInviteRepository>(Strict),
            userReleaseRoleAndInviteManager ?? Mock.Of<IUserReleaseRoleAndInviteManager>(Strict),
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions()
        );
    }
}
