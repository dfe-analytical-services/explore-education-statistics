#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        private readonly DataFixture _dataFixture = new();

        [Fact]
        public async Task ListPublicationsForUser()
        {
            var user = new User { Id = Guid.NewGuid() };

            Theme theme = _dataFixture.DefaultTheme();

            // Set up publication and release roles for the user

            UserPublicationRole userPublicationRoleApprover = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme))
                .WithRole(PublicationRole.Allower);

            UserPublicationRole userPublicationRoleOwner = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication().WithTheme(theme))
                .WithRole(PublicationRole.Owner);

            UserReleaseRole userReleaseRoleContributor = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(theme))))
                .WithRole(ReleaseRole.Contributor);

            UserReleaseRole userReleaseRolePreRelease = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(theme))))
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Themes.Add(theme);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoleApprover, userPublicationRoleOwner);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleContributor, userReleaseRolePreRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

                // Result should contain all publications except the one associated with the
                // Release.PrereleaseViewer role
                Assert.Equal(3, result.Count);

                Assert.Equal(userPublicationRoleApprover.PublicationId, result[0].Id);
                Assert.Empty(result[0].ReleaseVersions); // ListPublicationsForUser doesn't hydrate releases
                Assert.Empty(result[0].Methodologies); // ListPublicationsForUser doesn't hydrate methodologies

                Assert.Equal(userPublicationRoleOwner.PublicationId, result[1].Id);
                Assert.Empty(result[1].ReleaseVersions);
                Assert.Empty(result[1].Methodologies);

                Assert.Equal(userReleaseRoleContributor.ReleaseVersion.Release.PublicationId, result[2].Id);
                Assert.Empty(result[2].ReleaseVersions);
                Assert.Empty(result[2].Methodologies);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoThemeIdProvided()
        {
            var user = new User { Id = Guid.NewGuid() };

            // Set up publication and release roles for the user
            // Publications are all in different themes

            UserPublicationRole userPublicationRoleApprover = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithTheme(_dataFixture.DefaultTheme()))
                .WithRole(PublicationRole.Allower);

            UserPublicationRole userPublicationRoleOwner = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithTheme(_dataFixture.DefaultTheme()))
                .WithRole(PublicationRole.Owner);

            UserReleaseRole userReleaseRoleContributor = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(_dataFixture.DefaultTheme()))))
                .WithRole(ReleaseRole.Contributor);

            UserReleaseRole userReleaseRolePreRelease = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(_dataFixture.DefaultTheme()))))
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoleApprover, userPublicationRoleOwner);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoleContributor, userReleaseRolePreRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: null);

                // Result should contain all publications except the one associated with the
                // Release.PrereleaseViewer role
                Assert.Equal(3, result.Count);

                Assert.Equal(userPublicationRoleApprover.PublicationId, result[0].Id);
                Assert.Equal(userPublicationRoleOwner.PublicationId, result[1].Id);
                Assert.Equal(userReleaseRoleContributor.ReleaseVersion.Release.PublicationId, result[2].Id);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForTheme()
        {
            var user = new User { Id = Guid.NewGuid() };

            Theme theme = _dataFixture.DefaultTheme();

            // Set up other publication and release roles unrelated to this theme

            UserPublicationRole userPublicationRole = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithTheme(_dataFixture.DefaultTheme()))
                .WithRole(PublicationRole.Owner);

            UserReleaseRole userReleaseRole = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(_dataFixture.DefaultTheme()))))
                .WithRole(ReleaseRole.Contributor);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Themes.Add(theme);
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForUser()
        {
            var user = new User { Id = Guid.NewGuid() };

            Theme theme = _dataFixture.DefaultTheme();

            // Set up other publication and release roles unrelated to this user

            UserPublicationRole userPublicationRole = _dataFixture.DefaultUserPublicationRole()
                .WithUser(new User { Id = Guid.NewGuid() })
                .WithPublication(_dataFixture.DefaultPublication()
                    .WithTheme(theme))
                .WithRole(PublicationRole.Owner);

            UserReleaseRole userReleaseRole = _dataFixture.DefaultUserReleaseRole()
                .WithUser(new User { Id = Guid.NewGuid() })
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication()
                            .WithTheme(theme))))
                .WithRole(ReleaseRole.Contributor);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Themes.Add(theme);
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(userId: user.Id, themeId: theme.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_PublicationGrantedByBothPublicationAndReleaseRoles()
        {
            var user = new User { Id = Guid.NewGuid() };

            Publication publication = _dataFixture.DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme());

            // Check a Publication granted via the owner role is only returned once where it has a Release
            // also granted with roles to the same user

            UserPublicationRole userPublicationRole = _dataFixture.DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Owner);

            UserReleaseRole userReleaseRole = _dataFixture.DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(publication)))
                .WithRole(ReleaseRole.Contributor);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Publications.Add(publication);
                contentDbContext.UserReleaseRoles.Add(userReleaseRole);
                contentDbContext.UserPublicationRoles.Add(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var publications = await service.ListPublicationsForUser(userId: user.Id, themeId: publication.ThemeId);

                var resultPublication = Assert.Single(publications);
                Assert.Equal(publication.Id, resultPublication.Id);
            }
        }

        [Fact]
        public async Task QueryPublicationsForTheme()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Publications = [
                    new Publication { Title = "Theme 1 Publication 1", },
                    new Publication { Title = "Theme 1 Publication 2", }],
            };

            var theme2 = new Theme
            {
                Title = "Theme 2",
                Publications = [new() { Title = "Theme 2 Publication 1" }],
            };

            var theme3 = new Theme
            {
                Title = "Theme 3",
                Publications = [new() { Title = "Theme 3 Publication 1" }],
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(theme1, theme2, theme3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = new PublicationRepository(contentDbContext);
                var publications = await publicationService
                    .QueryPublicationsForTheme(theme1.Id)
                    .ToListAsync();

                Assert.Equal(2, publications.Count);
                Assert.Equal(theme1.Publications[0].Title, publications[0].Title);
                Assert.Equal(theme1.Publications[1].Title, publications[1].Title);
            }
        }
    }
}
