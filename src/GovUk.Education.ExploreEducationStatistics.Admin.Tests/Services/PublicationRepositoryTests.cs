#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        [Fact]
        public async Task ListPublicationsForUser()
        {
            var user = new User();
            var topic = new Topic
            {
                Theme = new Theme(),
            };

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();

            // Set up a publication and releases related to the topic that will be granted via different release roles
            var relatedPublication1 = new Publication
            {
                Title = "Related publication 1",
                Topic = topic,
            };
            userReleaseRoles.AddRange(new List<UserReleaseRole>
            {
                new()
                {
                    Release = new Release
                    {
                        ReleaseName = "2012",
                        TimePeriodCoverage = AcademicYear,
                        Publication = relatedPublication1
                    },
                    User = user,
                    Role = ReleaseRole.Viewer
                },
            });

            // Set up a publication and releases related to the topic that will be granted via the Publication
            // Owner role
            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Topic = topic,
                },
                User = user,
                Role = PublicationRole.Owner
            });

            // Set up a publication and releases related to the topic that will be granted via the Publication
            // Approver role
            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 3",
                    Topic = topic,
                },
                User = user,
                Role = PublicationRole.Approver
            });

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.Publications.AddAsync(relatedPublication1);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, topic.Id);

                // Result should contain Related publication 1, Related publication 2 and Related publication 3.
                // Related publication 4 is excluded because it's only granted via the PrereleaseViewer release role
                // Unrelated publications are excluded since they are for different topics
                Assert.Equal(3, result.Count);

                Assert.Equal("Related publication 2", result[0].Title);
                Assert.Empty(result[0].Releases);  // ListPublicationsForUser doesn't hydrate releases
                Assert.Empty(result[0].Methodologies); // ListPublicationsForUser doesn't hydrate methodologies

                Assert.Equal("Related publication 3", result[1].Title);
                Assert.Empty(result[1].Releases);
                Assert.Empty(result[1].Methodologies);

                Assert.Equal("Related publication 1", result[2].Title);
                Assert.Empty(result[2].Releases);
                Assert.Empty(result[2].Methodologies);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoTopicIdProvided()
        {
            var user = new User();

            var userPublicationRoles = new List<UserPublicationRole>
            {
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Owner publication",
                        Topic = new Topic { Theme = new Theme(), },
                    },
                    User = user,
                    Role = PublicationRole.Owner,
                },
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Approver publication",
                        Topic = new Topic { Theme = new Theme(), },
                    },
                    User = user,
                    Role = PublicationRole.Approver,
                },
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Publication Owner publication 2",
                        Topic = new Topic
                        {
                            Theme = new Theme(),
                        },
                    },
                    User = user,
                    Role = PublicationRole.Owner,
                },
            };

            var userReleaseRoles = new List<UserReleaseRole>
            {
                new()
                {
                    Release = new Release
                    {
                        ReleaseName = "2014",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Contributor publication",
                            Topic = new Topic { Theme = new Theme(), },
                        },
                    },
                    User = user,
                    Role = ReleaseRole.Contributor,
                },
                new()
                {
                    Release = new Release
                    {
                        ReleaseName = "2012",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Viewer publication",
                            Topic = new Topic { Theme = new Theme(), },
                        },
                    },
                    User = user,
                    Role = ReleaseRole.Viewer,
                },
                new()
                {
                    Release = new Release
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release PrereleaseViewer publication",
                            Topic = new Topic { Theme = new Theme(), },
                        }
                    },
                    User = user,
                    Role = ReleaseRole.PrereleaseViewer,
                },
                new()
                {
                    Release = new Release
                    {
                        ReleaseName = "2011",
                        TimePeriodCoverage = AcademicYear,
                        Publication = new Publication
                        {
                            Title = "Release Contributor publication 2",
                            Topic = new Topic
                            {
                                Theme = new Theme(),
                            },
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
                var result = await service.ListPublicationsForUser(user.Id, topicId: null);

                // Result should contain all publications except the one associated with the
                // Release.PrereleaseViewer role
                Assert.Equal(6, result.Count);

                Assert.False(result.Exists(pub => pub.Title == "Release PrereleaseViewer publication"));

                Assert.Equal("Publication Owner publication", result[0].Title);
                Assert.Empty(result[0].Releases);   // ListPublicationsForUser doesn't hydrate releases

                Assert.Equal("Publication Approver publication", result[1].Title);
                Assert.Empty(result[1].Releases);

                Assert.Equal("Publication Owner publication 2", result[2].Title);
                Assert.Empty(result[2].Releases);

                Assert.Equal("Release Contributor publication", result[3].Title);
                Assert.Empty(result[3].Releases);

                Assert.Equal("Release Viewer publication", result[4].Title);
                Assert.Empty(result[4].Releases);

                Assert.Equal("Release Contributor publication 2", result[5].Title);
                Assert.Empty(result[5].Releases);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForTopic()
        {
            var user = new User();
            var topic = new Topic();

            // Set up a publication and release unrelated to the topic that will be granted via a release role

            var userReleaseRole = new UserReleaseRole
            {
                Release = new Release
                {
                    ReleaseName = "2011",
                    TimePeriodCoverage = AcademicYear,
                    Publication = new Publication
                    {
                        Title = "Unrelated publication 1",
                        Topic = new Topic()
                    }
                },
                User = user,
                Role = ReleaseRole.Contributor
            };

            // Set up publication and release unrelated to the topic that will be granted via a publication role

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Unrelated publication 2",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = new Topic()
                },
                User = user,
                Role = PublicationRole.Owner
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, topic.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_NoPublicationsForUser()
        {
            var user = new User();
            var topic = new Topic();

            // Set up a publication and release related to the topic that is granted via a release role but not for this user

            var userReleaseRole = new UserReleaseRole
            {
                Release = new Release
                {
                    ReleaseName = "2011",
                    TimePeriodCoverage = AcademicYear,
                    Publication = new Publication
                    {
                        Title = "Related publication 1",
                        Topic = topic
                    }
                },
                UserId = new Guid(),
                Role = ReleaseRole.Contributor
            };

            // Set up a publication and release related to the topic that is granted via a publication role but not for this user

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = topic
                },
                UserId = new Guid(),
                Role = PublicationRole.Owner
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var result = await service.ListPublicationsForUser(user.Id, topic.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task ListPublicationsForUser_PublicationGrantedByBothPublicationAndReleaseRoles()
        {
            var user = new User();
            var topic = new Topic
            {
                Theme = new Theme(),
            };

            // Check a Publication granted via the owner role is only returned once where it has a Release
            // also granted with roles to the same user
            // Set up a publication and releases related to the topic that will be granted via different roles

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();

            var publication = new Publication
            {
                Title = "Publication",
                Topic = topic,
            };

            var release = new Release
            {
                ReleaseName = "2011",
                TimePeriodCoverage = AcademicYear,
                Publication = publication
            };

            userReleaseRoles.AddRange(new List<UserReleaseRole>
            {
                new()
                {
                    Release = release,
                    User = user,
                    Role = ReleaseRole.Contributor
                },
                new()
                {
                    Release = release,
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
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(contentDbContext);
                var publications = await service.ListPublicationsForUser(user.Id, topic.Id);

                var resultPublication = Assert.Single(publications);
                Assert.Equal("Publication", resultPublication.Title);

                var resultRelease = Assert.Single(resultPublication.Releases);
                Assert.Equal("Academic year 2011/12", resultRelease.Title);
            }
        }

        [Fact]
        public async Task QueryPublicationsForTopic()
        {
            var topic1 = new Topic
            {
                Title = "Topic 1",
                Theme = new Theme(),
                Publications = ListOf(
                    new Publication { Title = "Topic 1 Publication 1", },
                    new Publication { Title = "Topic 1 Publication 2", }),
            };

            var topic2 = new Topic
            {
                Title = "Topic 2",
                Theme = new Theme(),
                Publications = ListOf(new Publication { Title = "Topic 2 Publication 1" }),
            };

            var topic3 = new Topic
            {
                Title = "Topic 3",
                Theme = new Theme(),
                Publications = ListOf(new Publication { Title = "Topic 3 Publication 1" }),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Topics.AddRangeAsync(topic1, topic2, topic3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = new PublicationRepository(contentDbContext);
                var publications = await publicationService
                    .QueryPublicationsForTopic(topic1.Id)
                    .ToListAsync();

                Assert.Equal(2, publications.Count);
                Assert.Equal(topic1.Publications[0].Title, publications[0].Title);
                Assert.Equal(topic1.Publications[1].Title, publications[1].Title);
            }
        }

        [Fact]
        public async Task QueryPublicationsForTopic_NoTopic()
        {
            var topic1 = new Topic
            {
                Title = "Topic 1",
                Theme = new Theme(),
                Publications = ListOf(
                    new Publication { Title = "Topic 1 Publication 1", },
                    new Publication { Title = "Topic 1 Publication 2", }),
            };

            var topic2 = new Topic
            {
                Title = "Topic 2",
                Theme = new Theme(),
                Publications = ListOf(new Publication { Title = "Topic 2 Publication 1" }),
            };

            var topic3 = new Topic
            {
                Title = "Topic 3",
                Theme = new Theme(),
                Publications = ListOf(new Publication { Title = "Topic 3 Publication 1" }),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Topics.AddRangeAsync(topic1, topic2, topic3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = new PublicationRepository(contentDbContext);
                var publications = await publicationService
                    .QueryPublicationsForTopic()
                    .ToListAsync();

                Assert.Equal(4, publications.Count);
                Assert.Equal(topic1.Publications[0].Title, publications[0].Title);
                Assert.Equal(topic1.Publications[1].Title, publications[1].Title);
                Assert.Equal(topic2.Publications[0].Title, publications[2].Title);
                Assert.Equal(topic3.Publications[0].Title, publications[3].Title);
            }
        }
    }
}
