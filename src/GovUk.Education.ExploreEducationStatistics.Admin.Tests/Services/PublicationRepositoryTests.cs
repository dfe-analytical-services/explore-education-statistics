#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        [Fact]
        public async Task ListPublicationsForUser()
        {
            var user = new User();
            var theme = new Theme();

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();

            // Set up a publication and releases related to the theme that will be granted via different release roles
            var relatedPublication1 = new Publication
            {
                Title = "Related publication 1",
                Theme = theme,
            };
            userReleaseRoles.AddRange(new List<UserReleaseRole>
            {
                new()
                {
                    ReleaseVersion = new ReleaseVersion
                    {
                        ReleaseName = "2012",
                        TimePeriodCoverage = AcademicYear,
                        Publication = relatedPublication1
                    },
                    User = user,
                    Role = ReleaseRole.Viewer
                },
            });

            // Set up a publication and releases related to the theme that will be granted via the Publication
            // Owner role
            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Theme = theme,
                },
                User = user,
                Role = PublicationRole.Owner
            });

            // Set up a publication and releases related to the theme that will be granted via the Publication
            // Approver role
            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 3",
                    Theme = theme,
                },
                User = user,
                Role = PublicationRole.Approver
            });

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.Publications.AddAsync(relatedPublication1);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, theme.Id);

                // Result should contain Related publication 1, Related publication 2 and Related publication 3.
                // Related publication 4 is excluded because it's only granted via the PrereleaseViewer release role
                // Unrelated publications are excluded since they are for different themes
                Assert.Equal(3, result.Count);

                Assert.Equal("Related publication 2", result[0].Title);
                Assert.Empty(result[0].ReleaseVersions); // ListPublicationsForUser doesn't hydrate releases
                Assert.Empty(result[0].Methodologies); // ListPublicationsForUser doesn't hydrate methodologies

                Assert.Equal("Related publication 3", result[1].Title);
                Assert.Empty(result[1].ReleaseVersions);
                Assert.Empty(result[1].Methodologies);

                Assert.Equal("Related publication 1", result[2].Title);
                Assert.Empty(result[2].ReleaseVersions);
                Assert.Empty(result[2].Methodologies);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoThemeIdProvided()
        {
            var user = new User();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Owner publication",
                        Theme = new Theme(),
                    },
                    User = user,
                    Role = PublicationRole.Owner,
                },
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Approver publication",
                        Theme = new Theme(),
                    },
                    User = user,
                    Role = PublicationRole.Approver,
                },
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Owner publication 2",
                        Theme = new Theme(),
                    },
                    User = user,
                    Role = PublicationRole.Owner,
                },
            };

            var userReleaseRoles = new List<UserReleaseRole>
            {
                new()
                {
                    ReleaseVersion = new ReleaseVersion
                    {
                        ReleaseName = "2014",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Contributor publication",
                            Theme = new Theme(),
                        },
                    },
                    User = user,
                    Role = ReleaseRole.Contributor,
                },
                new()
                {
                    ReleaseVersion = new ReleaseVersion
                    {
                        ReleaseName = "2012",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Viewer publication",
                            Theme = new Theme(),
                        },
                    },
                    User = user,
                    Role = ReleaseRole.Viewer,
                },
                new()
                {
                    ReleaseVersion = new ReleaseVersion
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release PrereleaseViewer publication",
                            Theme = new Theme(),
                        }
                    },
                    User = user,
                    Role = ReleaseRole.PrereleaseViewer,
                },
                new()
                {
                    ReleaseVersion = new ReleaseVersion
                    {
                        ReleaseName = "2011",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Contributor publication 2",
                            Theme = new Theme(),
                        }
                    },
                    User = user,
                    Role = ReleaseRole.Contributor,
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, themeId: null);

                // Result should contain all publications except the one associated with the
                // Release.PrereleaseViewer role
                Assert.Equal(6, result.Count);

                Assert.False(result.Exists(pub => pub.Title == "Release PrereleaseViewer publication"));

                Assert.Equal("Publication Owner publication", result[0].Title);
                Assert.Equal("Publication Approver publication", result[1].Title);
                Assert.Equal("Publication Owner publication 2", result[2].Title);
                Assert.Equal("Release Contributor publication", result[3].Title);
                Assert.Equal("Release Viewer publication", result[4].Title);
                Assert.Equal("Release Contributor publication 2", result[5].Title);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForTheme()
        {
            var user = new User();
            var theme = new Theme();

            // Set up a publication and release unrelated to the theme that will be granted via a release role

            var userReleaseRole = new UserReleaseRole
            {
                ReleaseVersion = new ReleaseVersion
                {
                    ReleaseName = "2011",
                    TimePeriodCoverage = AcademicYear,
                    Publication = new Publication
                    {
                        Title = "Unrelated publication 1",
                        Theme = new Theme(),
                    }
                },
                User = user,
                Role = ReleaseRole.Contributor
            };

            // Set up publication and release unrelated to the theme that will be granted via a publication role

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Unrelated publication 2",
                    ReleaseVersions =
                    [
                        new()
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    ],
                    Theme = new Theme()
                },
                User = user,
                Role = PublicationRole.Owner
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, theme.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForUser()
        {
            var user = new User();
            var theme = new Theme();

            // Set up a publication and release related to the theme that is granted via a release role but not for this user

            var userReleaseRole = new UserReleaseRole
            {
                ReleaseVersion = new ReleaseVersion
                {
                    ReleaseName = "2011",
                    TimePeriodCoverage = AcademicYear,
                    Publication = new Publication
                    {
                        Title = "Related publication 1",
                        Theme = theme,
                    }
                },
                UserId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor
            };

            // Set up a publication and release related to the theme that is granted via a publication role but not for this user

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    ReleaseVersions =
                    [
                        new()
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    ],
                    Theme = theme,
                },
                UserId = Guid.NewGuid(),
                Role = PublicationRole.Owner
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, theme.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_PublicationGrantedByBothPublicationAndReleaseRoles()
        {
            var user = new User();
            var theme = new Theme();

            // Check a Publication granted via the owner role is only returned once where it has a Release
            // also granted with roles to the same user
            // Set up a publication and releases related to the theme that will be granted via different roles

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();

            var publication = new Publication
            {
                Title = "Publication",
                Theme = theme,
            };

            var releaseVersion = new ReleaseVersion
            {
                ReleaseName = "2011",
                TimePeriodCoverage = AcademicYear,
                Publication = publication
            };

            userReleaseRoles.AddRange(new List<UserReleaseRole>
            {
                new()
                {
                    ReleaseVersion = releaseVersion,
                    User = user,
                    Role = ReleaseRole.Contributor
                },
                new()
                {
                    ReleaseVersion = releaseVersion,
                    User = user,
                    Role = ReleaseRole.Lead
                }
            });

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = publication,
                User = user,
                Role = PublicationRole.Owner
            });

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Users.Add(user);
                contentDbContext.Themes.Add(theme);
                contentDbContext.Publications.Add(publication);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.UserReleaseRoles.AddRange(userReleaseRoles);
                contentDbContext.UserPublicationRoles.AddRange(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var publications = await service.ListPublicationsForUser(user.Id, theme.Id);

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
