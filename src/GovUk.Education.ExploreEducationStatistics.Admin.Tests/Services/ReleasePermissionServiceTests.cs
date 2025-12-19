#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using MockQueryable;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePermissionServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListReleaseRoles()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));
        ReleaseVersion otherReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(
                0,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            .ForIndex(
                1,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(PrereleaseViewer)
            )
            .ForIndex(2, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Approver))
            // This one should be ignored as it's for a different release version
            .ForIndex(
                3,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(otherReleaseVersion).SetRole(Contributor)
            )
            .GenerateList(4);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.ToArray().BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListReleaseRoles(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(3, viewModel.Count);

            Assert.Equal(userReleaseRoles[0].UserId, viewModel[0].UserId);
            Assert.Equal(userReleaseRoles[0].User.DisplayName, viewModel[0].UserDisplayName);
            Assert.Equal(userReleaseRoles[0].User.Email, viewModel[0].UserEmail);
            Assert.Equal(userReleaseRoles[0].Role, viewModel[0].Role);

            Assert.Equal(userReleaseRoles[1].UserId, viewModel[1].UserId);
            Assert.Equal(userReleaseRoles[1].User.DisplayName, viewModel[1].UserDisplayName);
            Assert.Equal(userReleaseRoles[1].User.Email, viewModel[1].UserEmail);
            Assert.Equal(userReleaseRoles[1].Role, viewModel[1].Role);

            Assert.Equal(userReleaseRoles[2].UserId, viewModel[2].UserId);
            Assert.Equal(userReleaseRoles[2].User.DisplayName, viewModel[2].UserDisplayName);
            Assert.Equal(userReleaseRoles[2].User.Email, viewModel[2].UserEmail);
            Assert.Equal(userReleaseRoles[2].Role, viewModel[2].Role);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
    }

    [Theory]
    [InlineData(Contributor)]
    [InlineData(PrereleaseViewer)]
    [InlineData(Approver)]
    [InlineData(Contributor, PrereleaseViewer)]
    public async Task ListReleaseRoles_SpecifiedRolesToInclude(params ReleaseRole[] rolesToInclude)
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(
                0,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            .ForIndex(
                1,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(PrereleaseViewer)
            )
            .ForIndex(2, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Approver))
            .GenerateList(3);

        var expectedRoles = userReleaseRoles.Where(urr => rolesToInclude.Contains(urr.Role)).ToList();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListReleaseRoles(releaseVersion.Id, rolesToInclude);
            var viewModel = result.AssertRight();

            Assert.Equal(expectedRoles.Count, viewModel.Count);
            Assert.All(expectedRoles, r => Assert.Contains(viewModel, urr => urr.Role == r.Role));
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
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
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(0, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Approver))
            .ForIndex(
                1,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            // This one should be ignored as it's for a different release version
            .ForIndex(
                2,
                s =>
                    s.SetUser(_dataFixture.DefaultUser())
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                )
                        )
                        .SetRole(Approver)
            )
            .GenerateList(3);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.PendingOnly))
            .Returns(userReleaseRoles.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListReleaseInvites(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Count);

            var userReleaseRolesOrderedByEmail = userReleaseRoles.OrderBy(urr => urr.User.Email).ToList();

            Assert.Equal(userReleaseRolesOrderedByEmail[0].User.Email, viewModel[0].Email);
            Assert.Equal(userReleaseRolesOrderedByEmail[0].Role, viewModel[0].Role);

            Assert.Equal(userReleaseRolesOrderedByEmail[1].User.Email, viewModel[1].Email);
            Assert.Equal(userReleaseRolesOrderedByEmail[1].Role, viewModel[1].Role);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListReleaseInvites_ContributorsOnly()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(
                0,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            // These two should be ignored as they are not Contributors
            .ForIndex(1, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Approver))
            .ForIndex(
                2,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(PrereleaseViewer)
            )
            .GenerateList(3);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.PendingOnly))
            .Returns(userReleaseRoles.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListReleaseInvites(releaseVersion.Id, rolesToInclude: [Contributor]);
            var viewModel = result.AssertRight();

            Assert.Single(viewModel);

            Assert.Equal(userReleaseRoles[0].User.Email, viewModel[0].Email);
            Assert.Equal(userReleaseRoles[0].Role, viewModel[0].Role);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
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
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.PendingOnly))
            .Returns(Array.Empty<UserReleaseRole>().BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListReleaseInvites(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListPublicationContributors()
    {
        Release release1 = _dataFixture.DefaultRelease(publishedVersions: 1);
        Release release2 = _dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true);
        Publication publication = _dataFixture.DefaultPublication().WithReleases(ListOf(release1, release2));
        User user1 = _dataFixture.DefaultUser();
        User user2 = _dataFixture.DefaultUser();

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(0, s => s.SetUser(user1).SetReleaseVersion(release1.Versions[0]).SetRole(Contributor))
            .ForIndex(1, s => s.SetUser(user2).SetReleaseVersion(release2.Versions[1]).SetRole(Contributor))
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleService = new Mock<IUserReleaseRoleService>();
        userReleaseRoleService
            .Setup(m => m.ListLatestActiveUserReleaseRolesByPublication(publication.Id, Contributor))
            .ReturnsAsync(userReleaseRoles);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await service.ListPublicationContributors(publication.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Count);

            Assert.Equal(user1.Id, viewModel[0].UserId);
            Assert.Equal(user1.DisplayName, viewModel[0].UserDisplayName);
            Assert.Equal(user1.Email, viewModel[0].UserEmail);

            Assert.Equal(user2.Id, viewModel[1].UserId);
            Assert.Equal(user2.DisplayName, viewModel[1].UserDisplayName);
            Assert.Equal(user2.Email, viewModel[1].UserEmail);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleService);
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
    public async Task ListPublicationContributors_NoUserReleaseRoles()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleService = new Mock<IUserReleaseRoleService>();
        userReleaseRoleService
            .Setup(m => m.ListLatestActiveUserReleaseRolesByPublication(publication.Id, Contributor))
            .ReturnsAsync([]);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await service.ListPublicationContributors(publication.Id);
            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }

        MockUtils.VerifyAllMocks(userReleaseRoleService);
    }

    [Fact]
    public async Task UpdateReleaseContributors()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            // This one should be removed as we are not supplying the userId for it
            .ForIndex(
                0,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            // This one should remain as we are supplying the userId for it
            .ForIndex(
                1,
                s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Contributor)
            )
            // This one should remain as it's an existing Approver role, and another Contributor role created for the user
            .ForIndex(2, s => s.SetUser(_dataFixture.DefaultUser()).SetReleaseVersion(releaseVersion).SetRole(Approver))
            // This one should remain as it's for a different release version, and another Contributor role created for the user
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
                        .SetRole(Contributor)
            )
            .GenerateList(4);

        var newUserId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());
        // The first role should be removed
        userReleaseRoleRepository
            .Setup(m =>
                m.RemoveMany(It.Is<List<UserReleaseRole>>(l => l.Single().Id == userReleaseRoles[0].Id), default)
            )
            .Returns(Task.CompletedTask);
        // The third role should be created
        userReleaseRoleRepository
            .Setup(m =>
                m.CreateManyIfNotExists(
                    It.Is<List<UserReleaseRole>>(l =>
                        l.Count == 3
                        && l[0].UserId == newUserId
                        && l[0].ReleaseVersionId == releaseVersion.Id
                        && l[0].Role == Contributor
                        && l[0].CreatedById == UserId
                        && Math.Abs((l[0].Created - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        && l[1].UserId == userReleaseRoles[2].UserId
                        && l[1].ReleaseVersionId == releaseVersion.Id
                        && l[1].Role == Contributor
                        && l[1].CreatedById == UserId
                        && Math.Abs((l[1].Created - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                        && l[2].UserId == userReleaseRoles[3].UserId
                        && l[2].ReleaseVersionId == releaseVersion.Id
                        && l[2].Role == Contributor
                        && l[2].CreatedById == UserId
                        && Math.Abs((l[2].Created - DateTime.UtcNow).Milliseconds) <= AssertExtensions.TimeWithinMillis
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.UpdateReleaseContributors(
                releaseVersion.Id,
                userIds: [newUserId, userReleaseRoles[1].UserId, userReleaseRoles[2].UserId, userReleaseRoles[3].UserId]
            );
            result.AssertRight();
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
    }

    [Fact]
    public async Task RemoveAllUserContributorPermissionForPublication()
    {
        Publication publication = _dataFixture.DefaultPublication();

        User user = _dataFixture.DefaultUser();

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .ForIndex(
                0,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                        )
                        .SetRole(Contributor)
            )
            .ForIndex(
                1,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                        )
                        .SetRole(Contributor)
            )
            // These ones should remain as it's not a Contributor role
            .ForIndex(
                2,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                        )
                        .SetRole(Approver)
            )
            .ForIndex(
                3,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                        )
                        .SetRole(PrereleaseViewer)
            )
            // This one should remain as it's for a different user
            .ForIndex(
                4,
                s =>
                    s.SetUser(_dataFixture.DefaultUser())
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
                        )
                        .SetRole(Contributor)
            )
            // This one should remain as it's for a different publication
            .ForIndex(
                5,
                s =>
                    s.SetUser(user)
                        .SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                )
                        )
                        .SetRole(Contributor)
            )
            .GenerateList(6);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(m => m.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());
        userReleaseRoleRepository
            .Setup(m =>
                m.RemoveMany(
                    It.Is<List<UserReleaseRole>>(l =>
                        l.Count == 2 && l[0].Id == userReleaseRoles[0].Id && l[1].Id == userReleaseRoles[1].Id
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupReleasePermissionService(
                contentDbContext: contentDbContext,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.RemoveAllUserContributorPermissionsForPublication(
                publicationId: publication.Id,
                userId: user.Id
            );

            result.AssertRight();
        }

        MockUtils.VerifyAllMocks(userReleaseRoleRepository);
    }

    private static ReleasePermissionService SetupReleasePermissionService(
        ContentDbContext? contentDbContext = null,
        IUserService? userService = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null,
        IUserReleaseRoleService? userReleaseRoleService = null
    )
    {
        contentDbContext ??= InMemoryApplicationDbContext();

        return new(
            persistenceHelper: new PersistenceHelper<ContentDbContext>(contentDbContext),
            userReleaseRoleRepository: userReleaseRoleRepository
                ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict),
            userReleaseRoleService: userReleaseRoleService ?? Mock.Of<IUserReleaseRoleService>(MockBehavior.Strict),
            userService: userService ?? MockUtils.AlwaysTrueUserService(UserId).Object
        );
    }
}
