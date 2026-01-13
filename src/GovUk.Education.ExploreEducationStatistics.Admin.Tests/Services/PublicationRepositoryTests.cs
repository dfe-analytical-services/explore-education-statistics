#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PublicationRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListPublicationsForUser()
    {
        User user = _dataFixture.DefaultUser();

        var (theme1, theme2) = _dataFixture.DefaultTheme().GenerateTuple2();

        var userPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetPublication(_dataFixture.DefaultPublication().WithTheme(theme1))
                        .SetRole(PublicationRole.Allower)
            )
            .ForIndex(
                1,
                s =>
                    s.SetPublication(_dataFixture.DefaultPublication().WithTheme(theme1)).SetRole(PublicationRole.Owner)
            )
            // This one should be excluded from the results due to being for a different theme
            .ForIndex(
                2,
                s =>
                    s.SetPublication(_dataFixture.DefaultPublication().WithTheme(theme2)).SetRole(PublicationRole.Owner)
            )
            .GenerateList(3);

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme1))
                                )
                        )
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                1,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme1))
                                )
                        )
                        .SetRole(ReleaseRole.Approver)
            )
            // This one should be excluded from results due to it being for a PrereleaseViewer role
            .ForIndex(
                2,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme1))
                                )
                        )
                        .SetRole(ReleaseRole.PrereleaseViewer)
            )
            .ForIndex(
                3,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme2))
                                )
                        )
                        .SetRole(ReleaseRole.Contributor)
            )
            // This one should be excluded from the results due to being for a different theme
            .GenerateList(4);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userPublicationRoles.BuildMock());

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme1.Id);

            // Result should contain all publications except the one associated with the
            // Release.PrereleaseViewer role, and the ones for theme2
            Assert.Equal(4, result.Count);

            Assert.Equal(userPublicationRoles[0].PublicationId, result[0].Id);
            Assert.Empty(result[0].ReleaseVersions); // ListPublicationsForUser doesn't hydrate releases
            Assert.Empty(result[0].Methodologies); // ListPublicationsForUser doesn't hydrate methodologies

            Assert.Equal(userPublicationRoles[1].PublicationId, result[1].Id);
            Assert.Empty(result[1].ReleaseVersions);
            Assert.Empty(result[1].Methodologies);

            Assert.Equal(userReleaseRoles[0].ReleaseVersion.Release.PublicationId, result[2].Id);
            Assert.Empty(result[2].ReleaseVersions);
            Assert.Empty(result[2].Methodologies);

            Assert.Equal(userReleaseRoles[1].ReleaseVersion.Release.PublicationId, result[3].Id);
            Assert.Empty(result[3].ReleaseVersions);
            Assert.Empty(result[3].Methodologies);
        }

        MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListPublicationsForUser_NoThemeIdProvided()
    {
        User user = _dataFixture.DefaultUser();

        var userPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                        .SetRole(PublicationRole.Allower)
            )
            .ForIndex(
                1,
                s =>
                    s.SetPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                        .SetRole(PublicationRole.Owner)
            )
            .GenerateList(2);

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(
                                            _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme())
                                        )
                                )
                        )
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                1,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(
                                            _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme())
                                        )
                                )
                        )
                        .SetRole(ReleaseRole.Approver)
            )
            // This one should be excluded from results
            .ForIndex(
                2,
                s =>
                    s.SetReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(
                                            _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme())
                                        )
                                )
                        )
                        .SetRole(ReleaseRole.PrereleaseViewer)
            )
            .GenerateList(3);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
            contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
            await contentDbContext.SaveChangesAsync();
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userPublicationRoles.BuildMock());

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(userReleaseRoles.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListPublicationsForUser(userId: user.Id);

            // Result should contain all publications except the one associated with the
            // Release.PrereleaseViewer role
            Assert.Equal(4, result.Count);

            Assert.Equal(userPublicationRoles[0].PublicationId, result[0].Id);
            Assert.Equal(userPublicationRoles[1].PublicationId, result[1].Id);
            Assert.Equal(userReleaseRoles[0].ReleaseVersion.Release.PublicationId, result[2].Id);
            Assert.Equal(userReleaseRoles[1].ReleaseVersion.Release.PublicationId, result[3].Id);
        }

        MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListPublicationsForUser_NoPublicationsForTheme()
    {
        User user = _dataFixture.DefaultUser();

        Theme theme = _dataFixture.DefaultTheme();

        // Set up other publication and release roles unrelated to this theme

        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
            .WithRole(PublicationRole.Owner);

        UserReleaseRole userReleaseRole = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(
                _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                    )
            )
            .WithRole(ReleaseRole.Contributor);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.Themes.Add(theme);
            contentDbContext.UserReleaseRoles.Add(userReleaseRole);
            contentDbContext.UserPublicationRoles.Add(userPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userPublicationRole }.BuildMock());

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userReleaseRole }.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

            Assert.Empty(result);
        }

        MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListPublicationsForUser_NoPublicationsForUser()
    {
        User user = _dataFixture.DefaultUser();

        Theme theme = _dataFixture.DefaultTheme();

        // Set up other publication and release roles unrelated to this user

        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme))
            .WithRole(PublicationRole.Owner);

        UserReleaseRole userReleaseRole = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(_dataFixture.DefaultUser())
            .WithReleaseVersion(
                _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme))
                    )
            )
            .WithRole(ReleaseRole.Contributor);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.UserReleaseRoles.Add(userReleaseRole);
            contentDbContext.UserPublicationRoles.Add(userPublicationRole);
            await contentDbContext.SaveChangesAsync();
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userPublicationRole }.BuildMock());

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userReleaseRole }.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

            Assert.Empty(result);
        }

        MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task ListPublicationsForUser_PublicationGrantedByBothPublicationAndReleaseRoles()
    {
        User user = _dataFixture.DefaultUser();

        Publication publication = _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme());

        // Check a Publication granted via the owner role is only returned once where it has a Release
        // also granted with roles to the same user

        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithPublication(publication)
            .WithRole(PublicationRole.Owner);

        UserReleaseRole userReleaseRole = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithReleaseVersion(
                _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication))
            )
            .WithRole(ReleaseRole.Contributor);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.Users.Add(user);
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
        userPublicationRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userPublicationRole }.BuildMock());

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>();
        userReleaseRoleRepository
            .Setup(r => r.Query(ResourceRoleFilter.ActiveOnly))
            .Returns(new[] { userReleaseRole }.BuildMock());

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                userReleaseRoleRepository: userReleaseRoleRepository.Object
            );

            var publications = await service.ListPublicationsForUser(userId: user.Id, themeId: publication.ThemeId);

            var resultPublication = Assert.Single(publications);
            Assert.Equal(publication.Id, resultPublication.Id);
        }

        MockUtils.VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task QueryPublicationsForTheme()
    {
        var theme1 = new Theme
        {
            Title = "Theme 1",
            Publications =
            [
                new Publication { Title = "Theme 1 Publication 1" },
                new Publication { Title = "Theme 1 Publication 2" },
            ],
        };

        var theme2 = new Theme { Title = "Theme 2", Publications = [new() { Title = "Theme 2 Publication 1" }] };

        var theme3 = new Theme { Title = "Theme 3", Publications = [new() { Title = "Theme 3 Publication 1" }] };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Themes.AddRangeAsync(theme1, theme2, theme3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildService(contentDbContext);
            var publications = await publicationService.QueryPublicationsForTheme(theme1.Id).ToListAsync();

            Assert.Equal(2, publications.Count);
            Assert.Equal(theme1.Publications[0].Title, publications[0].Title);
            Assert.Equal(theme1.Publications[1].Title, publications[1].Title);
        }
    }

    private static PublicationRepository BuildService(
        ContentDbContext? contentDbContext = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        return new(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(MockBehavior.Strict)
        );
    }
}
