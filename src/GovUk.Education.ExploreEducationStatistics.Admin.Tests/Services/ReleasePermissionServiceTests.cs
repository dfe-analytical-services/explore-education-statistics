#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using ReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePermissionServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListReleaseRoles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        UserReleaseRole user1ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User
            {
                FirstName = "User1",
                LastName = "One",
                Email = "user1@test.com",
            })
            .WithReleaseVersion(releaseVersion)
            .WithRole(Contributor);

        UserReleaseRole user2ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User
            {
                FirstName = "User2",
                LastName = "Two",
                Email = "user2@test.com",
            })
            .WithReleaseVersion(releaseVersion)
            .WithRole(PrereleaseViewer);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>();
        userReleaseRoleAndInviteManager
            .Setup(m => m.ListUserReleaseRoles(releaseVersion.Id, null))
            .ReturnsAsync([user1ReleaseRole, user2ReleaseRole])
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object);

            var result = await service.ListReleaseRoles(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Count);

            Assert.Equal(user1ReleaseRole.UserId, viewModel[0].UserId);
            Assert.Equal(user1ReleaseRole.User.DisplayName, viewModel[0].UserDisplayName);
            Assert.Equal(user1ReleaseRole.User.Email, viewModel[0].UserEmail);
            Assert.Equal(user1ReleaseRole.Role, viewModel[0].Role);

            Assert.Equal(user2ReleaseRole.UserId, viewModel[1].UserId);
            Assert.Equal(user2ReleaseRole.User.DisplayName, viewModel[1].UserDisplayName);
            Assert.Equal(user2ReleaseRole.User.Email, viewModel[1].UserEmail);
            Assert.Equal(user2ReleaseRole.Role, viewModel[1].Role);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleAndInviteManager);
    }

    [Theory]
    [InlineData(Contributor)]
    [InlineData(PrereleaseViewer)]
    [InlineData(Approver)]
    [InlineData(Contributor, PrereleaseViewer)]
    public async Task ListReleaseRoles_SpecifiedRolesToInclude(params ReleaseRole[] rolesToInclude)
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>();
        userReleaseRoleAndInviteManager
            .Setup(m => m.ListUserReleaseRoles(releaseVersion.Id, rolesToInclude))
            .ReturnsAsync([])
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object);

            var result = await service.ListReleaseRoles(releaseVersion.Id, rolesToInclude);
            var viewModel = result.AssertRight();
        }

        MockUtils.VerifyAllMocks(userReleaseRoleAndInviteManager);
    }

    [Fact]
    public async Task ListReleaseRoles_NoReleaseVersion()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupReleasePermissionService(contentDbContext);

        var result = await service.ListReleaseRoles(releaseVersionId: Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListReleaseInvites()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var (userReleaseInvite1, userReleaseInvite2) = _dataFixture.DefaultUserReleaseInvite()
            .WithReleaseVersion(releaseVersion)
            .WithRoles([Contributor, Approver])
            .GenerateTuple2();

        UserReleaseInvite userReleaseInviteIgnored = _dataFixture.DefaultUserReleaseInvite()
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication())))
            .WithRole(Contributor);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseInvites.AddRange(userReleaseInvite1,
                userReleaseInvite2,
                userReleaseInviteIgnored);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseInvites(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Count);

            Assert.Equal(userReleaseInvite1.Email, viewModel[0].Email);
            Assert.Equal(userReleaseInvite1.Role, viewModel[0].Role);

            Assert.Equal(userReleaseInvite2.Email, viewModel[1].Email);
            Assert.Equal(userReleaseInvite2.Role, viewModel[1].Role);
        }
    }

    [Fact]
    public async Task ListReleaseInvites_ContributorsOnly()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var (userReleaseInvite1, userReleaseInvite2) = _dataFixture.DefaultUserReleaseInvite()
            .WithReleaseVersion(releaseVersion)
            .WithRoles([Contributor, Approver])
            .GenerateTuple2();

        UserReleaseInvite userReleaseInviteIgnored = _dataFixture.DefaultUserReleaseInvite()
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication())))
            .WithRole(Contributor);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseInvites.AddRange(userReleaseInvite1,
                userReleaseInvite2,
                userReleaseInviteIgnored);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseInvites(releaseVersion.Id, rolesToInclude: [Contributor]);
            var viewModel = result.AssertRight();

            Assert.Single(viewModel);

            Assert.Equal(userReleaseInvite1.Email, viewModel[0].Email);
            Assert.Equal(userReleaseInvite1.Role, viewModel[0].Role);
        }
    }

    [Fact]
    public async Task ListReleaseInvites_NoPublication()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupReleasePermissionService(contentDbContext);

        var result = await service.ListReleaseInvites(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListReleaseInvites_NoReleaseVersion()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupReleasePermissionService(contentDbContext);

        var result = await service.ListReleaseInvites(releaseVersionId: Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListReleaseInvites_NoUserReleaseInvites()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseInvites(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
    }

    [Fact]
    public async Task ListPublicationContributors()
    {
        Release release1 = _dataFixture
            .DefaultRelease(publishedVersions: 1);

        Release release2 = _dataFixture
            .DefaultRelease(publishedVersions: 1, draftVersion: true);

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(ListOf(release1, release2));

        var user1 = new User
        {
            FirstName = "User1",
            LastName = "One",
            Email = "user1@test.com",
        };
        var user1ReleaseRole1 = new UserReleaseRole
        {
            User = user1,
            ReleaseVersion = release1.Versions[0],
            Role = Contributor,
        };

        var user2 = new User
        {
            FirstName = "User2",
            LastName = "Two",
            Email = "user2@test.com",
        };
        var user2ReleaseRole1 = new UserReleaseRole
        {
            User = user2,
            ReleaseVersion = release2.Versions[1],
            Role = Contributor,
        };

        var user3 = new User();
        var user3ReleaseRoleIgnored1 = new UserReleaseRole // Ignored because different publication
        {
            User = user3,
            ReleaseVersion = new ReleaseVersion { Publication = new Publication() },
            Role = Contributor,
        };
        var user3ReleaseRoleIgnored2 = new UserReleaseRole // Ignored because not Contributor role
        {
            User = user3,
            ReleaseVersion = release1.Versions[0],
            Role = PrereleaseViewer,
        };
        var user3ReleaseRoleIgnored3 = new UserReleaseRole // Ignored because not latest version of release
        {
            User = user3,
            ReleaseVersion = release2.Versions[0],
            Role = Contributor,
            Deleted = DateTime.UtcNow,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.UserReleaseRoles.AddRange(
                user1ReleaseRole1,
                user2ReleaseRole1,
                user3ReleaseRoleIgnored1,
                user3ReleaseRoleIgnored2,
                user3ReleaseRoleIgnored3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result =
                await service.ListPublicationContributors(publication.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Count);

            Assert.Equal(user1.Id, viewModel[0].UserId);
            Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
            Assert.Equal(user1.Email, viewModel[0].UserEmail);

            Assert.Equal(user2.Id, viewModel[1].UserId);
            Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
            Assert.Equal(user2.Email, viewModel[1].UserEmail);
        }
    }

    [Fact]
    public async Task ListPublicationContributors_NoPublication()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupReleasePermissionService(contentDbContext);

        var result = await service.ListPublicationContributors(publicationId: Guid.NewGuid());
        result.AssertNotFound();
    }


    [Fact]
    public async Task ListPublicationContributors_NoReleaseVersion()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListPublicationContributors(publication.Id);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
    }

    [Fact]
    public async Task ListPublicationContributors_NoUserReleaseRoles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result =
                await service.ListPublicationContributors(publicationId: releaseVersion.Release.PublicationId);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
    }

    [Fact]
    public async Task UpdateReleaseContributors()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        UserReleaseRole user1ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User { Id = Guid.NewGuid() })
            .WithReleaseVersion(releaseVersion)
            .WithRole(Contributor);

        UserReleaseRole user2ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User { Id = Guid.NewGuid(), Email = "test@test.com" })
            .WithReleaseVersion(releaseVersion)
            .WithRole(Contributor);

        var user3 = new User { Id = Guid.NewGuid() };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseRoles.AddRange(user1ReleaseRole, user2ReleaseRole);
            contentDbContext.Users.Add(user3);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>();
        // User 2's role should be removed
        userReleaseRoleAndInviteManager
            .Setup(m => m.RemoveRolesAndInvites(
                It.Is<List<UserReleaseRole>>(l => l.Single().Id == user2ReleaseRole.Id),
                default))
            .Returns(Task.CompletedTask)
            .Verifiable();
        // User 3's role should be created
        userReleaseRoleAndInviteManager
            .Setup(m => m.CreateManyIfNotExists(
                It.Is<List<Guid>>(l => l.Single() == user3.Id),
                releaseVersion.Id,
                Contributor,
                UserId))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object);

            var result =
                await service.UpdateReleaseContributors(releaseVersion.Id,
                    userIds: [user1ReleaseRole.UserId, user3.Id]);
            result.AssertRight();
        }

        MockUtils.VerifyAllMocks(userReleaseRoleAndInviteManager);
    }

    [Fact]
    public async Task RemoveAllUserContributorPermissionForPublication()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var userId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleAndInviteManager = new Mock<IUserReleaseRoleAndInviteManager>();
        userReleaseRoleAndInviteManager
            .Setup(m => m.RemoveAllRolesAndInvitesForPublication(
                publication.Id,
                userId,
                default,
                Contributor))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager.Object);

            var result = await service.RemoveAllUserContributorPermissionsForPublication(
                publicationId: publication.Id,
                userId: userId);

            result.AssertRight();
        }

        MockUtils.VerifyAllMocks(userReleaseRoleAndInviteManager);
    }

    private static ReleasePermissionService SetupReleasePermissionService(
        ContentDbContext contentDbContext,
        IUserService? userService = null,
        IUserReleaseRoleAndInviteManager? userReleaseRoleAndInviteManager = null)
    {
        return new(
            contentDbContext: contentDbContext,
            persistenceHelper: new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
            userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager ?? Mock.Of<IUserReleaseRoleAndInviteManager>(MockBehavior.Strict),
            userService: userService ?? MockUtils.AlwaysTrueUserService(UserId).Object
        );
    }
}
