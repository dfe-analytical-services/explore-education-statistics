#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class PublicationRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public class ListPublicationsForUserTests : PublicationRepositoryTests
    {
        [Fact]
        public async Task ThemeIdProvided()
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
                            .SetRole(PublicationRole.Approver)
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublication(_dataFixture.DefaultPublication().WithTheme(theme1))
                            .SetRole(PublicationRole.Drafter)
                )
                // This one should be excluded from the results due to being for a different theme
                .ForIndex(
                    2,
                    s =>
                        s.SetPublication(_dataFixture.DefaultPublication().WithTheme(theme2))
                            .SetRole(PublicationRole.Drafter)
                )
                .GenerateList(3);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme1.Id);

                // Result should contain all publications except the one associated with theme2
                Assert.Equal(2, result.Count);

                Assert.Equal(userPublicationRoles[0].PublicationId, result[0].Id);
                Assert.Equal(theme1.Id, result[0].Theme.Id);
                Assert.Empty(result[0].ReleaseVersions); // ListPublicationsForUser doesn't hydrate releases
                Assert.Empty(result[0].Methodologies); // ListPublicationsForUser doesn't hydrate methodologies

                Assert.Equal(userPublicationRoles[1].PublicationId, result[1].Id);
                Assert.Equal(theme1.Id, result[1].Theme.Id);
                Assert.Empty(result[1].ReleaseVersions);
                Assert.Empty(result[1].Methodologies);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task NoThemeIdProvided()
        {
            User user = _dataFixture.DefaultUser();

            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .ForIndex(
                    0,
                    s =>
                        s.SetPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                            .SetRole(PublicationRole.Approver)
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                            .SetRole(PublicationRole.Drafter)
                )
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.ListPublicationsForUser(userId: user.Id);

                // Result should contain all publications
                Assert.Equal(2, result.Count);

                Assert.Equal(userPublicationRoles[0].PublicationId, result[0].Id);
                Assert.Equal(userPublicationRoles[0].Publication.ThemeId, result[0].Theme.Id);
                Assert.Equal(userPublicationRoles[1].PublicationId, result[1].Id);
                Assert.Equal(userPublicationRoles[1].Publication.ThemeId, result[1].Theme.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task NoPublicationsForTheme()
        {
            User user = _dataFixture.DefaultUser();

            Theme theme = _dataFixture.DefaultTheme();

            // Set up other publication roles unrelated to this theme

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
                .WithRole(PublicationRole.Drafter);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Themes.Add(theme);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task NoPublicationsForUser()
        {
            User user = _dataFixture.DefaultUser();

            Theme theme = _dataFixture.DefaultTheme();

            // Set up other publication roles unrelated to this user

            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(_dataFixture.DefaultUser())
                .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme))
                .WithRole(PublicationRole.Drafter);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, userPublicationRole);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }

        [Fact]
        public async Task PublicationGrantedByMultiplePublicationRoles()
        {
            User user = _dataFixture.DefaultUser();

            Publication publication = _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme());

            // Check a Publication granted via BOTH the 'Drafter' role AND 'Approver' role is only returned once

            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Drafter))
                .ForIndex(1, s => s.SetRole(PublicationRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>();
            userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, [.. userPublicationRoles]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var publications = await service.ListPublicationsForUser(userId: user.Id, themeId: publication.ThemeId);

                var resultPublication = Assert.Single(publications);
                Assert.Equal(publication.Id, resultPublication.Id);
            }

            MockUtils.VerifyAllMocks(userPublicationRoleRepository);
        }
    }

    public class QueryPublicationsForThemeTests : PublicationRepositoryTests
    {
        [Fact]
        public async Task Success()
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
    }

    private static PublicationRepository BuildService(
        ContentDbContext? contentDbContext = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null
    )
    {
        return new(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict)
        );
    }
}
