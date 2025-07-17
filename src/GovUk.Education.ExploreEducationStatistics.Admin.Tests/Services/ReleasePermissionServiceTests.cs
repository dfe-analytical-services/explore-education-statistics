#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
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

        UserReleaseRole user3ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User
            {
                FirstName = "User3",
                LastName = "Three",
                Email = "user3@test.com",
            })
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication())))
            .WithRole(Contributor);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseRoles.AddRange(user1ReleaseRole, user2ReleaseRole, user3ReleaseRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

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
    }

    [Fact]
    public async Task ListReleaseRoles_ContributorsOnly()
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

        UserReleaseRole user3ReleaseRole = _dataFixture.DefaultUserReleaseRole()
            .WithUser(new User
            {
                FirstName = "User3",
                LastName = "Three",
                Email = "user3@test.com",
            })
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication())))
            .WithRole(Contributor);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseRoles.AddRange(user1ReleaseRole, user2ReleaseRole, user3ReleaseRole);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.ListReleaseRoles(releaseVersion.Id, rolesToInclude: [Contributor]);
            var viewModel = result.AssertRight();

            Assert.Single(viewModel);

            Assert.Equal(user1ReleaseRole.UserId, viewModel[0].UserId);
            Assert.Equal(user1ReleaseRole.User.DisplayName, viewModel[0].UserDisplayName);
            Assert.Equal(user1ReleaseRole.User.Email, viewModel[0].UserEmail);
            Assert.Equal(user1ReleaseRole.Role, viewModel[0].Role);
        }
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
    public async Task ListReleaseRoles_NoUserReleaseRoles()
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

            var result = await service.ListReleaseRoles(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
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

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result =
                await service.UpdateReleaseContributors(releaseVersion.Id,
                    userIds: [user1ReleaseRole.UserId, user3.Id]);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualUserReleaseRoles = await contentDbContext.UserReleaseRoles.ToListAsync();

            Assert.Equal(2, actualUserReleaseRoles.Count);

            // User 1's role should be untouched
            Assert.Equal(user1ReleaseRole.Id, actualUserReleaseRoles[0].Id);

            // User 3's role should be created
            Assert.Equal(user3.Id, actualUserReleaseRoles[1].UserId);
            Assert.Equal(Contributor, actualUserReleaseRoles[1].Role);
            actualUserReleaseRoles[1].Created.AssertUtcNow();
            Assert.Equal(UserId, actualUserReleaseRoles[1].CreatedById);
        }
    }

    [Fact]
    public async Task UpdateReleaseContributors_RemoveAllContributorsFromRelease()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture.DefaultUserReleaseRole()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s.SetUser(new User { Id = Guid.NewGuid(), Email = "test1@test.com" }))
            .ForIndex(1, s => s.SetUser(new User { Id = Guid.NewGuid(), Email = "test2@test.com" }))
            .WithRole(Contributor)
            .Generate(2);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result = await service.UpdateReleaseContributors(releaseVersion.Id, userIds: []);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualUserReleaseRoles = await contentDbContext.UserReleaseRoles
                .ToListAsync();

            Assert.Empty(actualUserReleaseRoles);
        }
    }

    [Fact]
    public async Task RemoveAllUserContributorPermissionForPublication()
    {
        Publication publication = _dataFixture.DefaultPublication()
            .WithReleases(_ =>
            [
                _dataFixture.DefaultRelease(publishedVersions: 2),
                _dataFixture.DefaultRelease(publishedVersions: 2)
            ]);

        var user1 = new User
        {
            FirstName = "User1",
            LastName = "One",
            Email = "user1@test.com",
        };

        var user1ReleaseRoles = _dataFixture.DefaultUserReleaseRole()
            .WithRole(Contributor)
            .WithReleaseVersions(publication.Releases[0].Versions)
            .WithUser(user1)
            .GenerateList(2);

        var user2 = new User
        {
            FirstName = "User2",
            LastName = "Two",
            Email = "user2@test.com",
        };

        var user2ReleaseRoles = _dataFixture.DefaultUserReleaseRole()
            .WithRole(Contributor)
            .WithReleaseVersions(publication.Releases[1].Versions)
            .WithUser(user2)
            .GenerateList(2);

        var allUserReleaseRoles = user1ReleaseRoles.Concat(user2ReleaseRoles).ToList();

        UserReleaseInvite user1Release1Invite = _dataFixture.DefaultUserReleaseInvite()
            .WithEmail(user1.Email)
            .WithReleaseVersion(publication.Releases[0].Versions[0])
            .WithRole(Contributor);

        UserReleaseInvite user2Release1Invite = _dataFixture.DefaultUserReleaseInvite()
            .WithEmail(user2.Email)
            .WithReleaseVersion(publication.Releases[1].Versions[0])
            .WithRole(Contributor);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.UserReleaseRoles.AddRange(allUserReleaseRoles);
            contentDbContext.UserReleaseInvites.AddRange(user1Release1Invite, user2Release1Invite);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(contentDbContext);

            var result =
                await service.RemoveAllUserContributorPermissionsForPublication(publicationId: publication.Id,
                    userId: user2.Id);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualReleaseRoles = await contentDbContext.UserReleaseRoles
                .ToListAsync();

            // User 1's release roles should remain
            Assert.Equal(2, actualReleaseRoles.Count);
            Assert.Equal(user1ReleaseRoles[0].Id, actualReleaseRoles[0].Id);
            Assert.Equal(user1ReleaseRoles[1].Id, actualReleaseRoles[1].Id);

            var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites
                .ToListAsync();

            // User 1's release invite should remain
            Assert.Single(actualUserReleaseInvites);
            Assert.Equal(user1Release1Invite.Id, actualUserReleaseInvites[0].Id);
        }
    }

    private static ReleasePermissionService SetupReleasePermissionService(
        ContentDbContext contentDbContext,
        IUserService? userService = null)
    {
        var userRepository = new UserRepository(contentDbContext);

        var userReleaseRoleAndInviteManager = new UserReleaseRoleAndInviteManager(
            contentDbContext,
            new UserReleaseInviteRepository(contentDbContext),
            userRepository);

        return new(
            contentDbContext: contentDbContext,
            persistenceHelper: new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
            userReleaseRoleAndInviteManager: userReleaseRoleAndInviteManager,
            userService: userService ?? MockUtils.AlwaysTrueUserService(UserId).Object
        );
    }
}
