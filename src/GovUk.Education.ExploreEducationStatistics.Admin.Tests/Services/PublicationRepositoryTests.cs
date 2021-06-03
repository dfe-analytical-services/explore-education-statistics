using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser()
        {
            var user = new User();
            var topic = new Topic();

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();
            
            // Set up a publication and releases related to the topic that will be granted via different release roles

            var relatedPublication1 = new Publication
            {
                Title = "Related publication 1",
                Topic = topic
            };

            userReleaseRoles.AddRange(new List<UserReleaseRole>
            {
                new UserReleaseRole
                {
                    Release = new Release
                    {
                        ReleaseName = "2011",
                        TimePeriodCoverage = AcademicYear,
                        Publication = relatedPublication1
                    },
                    User = user,
                    Role = Contributor
                },
                new UserReleaseRole
                {
                    Release = new Release
                    {
                        ReleaseName = "2012",
                        TimePeriodCoverage = AcademicYear,
                        Publication = relatedPublication1
                    },
                    User = user,
                    Role = Viewer
                },
                new UserReleaseRole
                {
                    Release = new Release
                    {
                        ReleaseName = "2013",
                        TimePeriodCoverage = AcademicYear,
                        Publication = relatedPublication1
                    },
                    User = user,
                    Role = PrereleaseViewer
                }
            });

            // Set up a publication and releases related to the topic that will be granted via a publication role

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            ReleaseName = "2015",
                            TimePeriodCoverage = AcademicYear
                        },
                        new Release
                        {
                            ReleaseName = "2016",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = topic
                },
                User = user,
                Role = Owner
            });
            
            // Set up a publication and release related to the topic that will be granted solely via the PrereleaseViewer role

            userReleaseRoles.Add(new UserReleaseRole
            {
                Release = new Release
                {
                    ReleaseName = "2014",
                    TimePeriodCoverage = AcademicYear,
                    Publication = new Publication
                    {
                        Title = "Related publication 3",
                        Topic = topic
                    }
                },
                User = user,
                Role = PrereleaseViewer
            });

            // Set up a publication and release unrelated to the topic that will be granted via a release role

            userReleaseRoles.Add(new UserReleaseRole
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
                Role = Contributor
            });

            // Set up publication and release unrelated to the topic that will be granted via a publication role

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Unrelated publication 2",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = new Topic()
                },
                User = user,
                Role = Owner
            });

            var contextId = Guid.NewGuid().ToString();

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.Publications.AddAsync(relatedPublication1);
                await contentDbContext.UserReleaseRoles.AddRangeAsync(userReleaseRoles);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUserAsync(topic.Id, user.Id);

                // Result should contain Related publication 1 and Related publication 2
                // Related publication 3 is excluded because it's only granted via the PrereleaseViewer release role
                // Unrelated publications are excluded since they are for different topics
                Assert.Equal(2, result.Count);
                Assert.Equal("Related publication 1", result[0].Title);
                Assert.Equal("Related publication 2", result[1].Title);
            }
        }

        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser_NoPublicationsForTopic()
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
                Role = Contributor
            };

            // Set up publication and release unrelated to the topic that will be granted via a publication role

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Unrelated publication 2",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = new Topic()
                },
                User = user,
                Role = Owner
            };

            var contextId = Guid.NewGuid().ToString();

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUserAsync(topic.Id, user.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser_NoPublicationsForUser()
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
                Role = Contributor
            };

            // Set up a publication and release related to the topic that is granted via a publication role but not for this user

            var userPublicationRole = new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            ReleaseName = "2012",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = topic
                },
                UserId = new Guid(),
                Role = Owner
            };

            var contextId = Guid.NewGuid().ToString();

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole);
                await contentDbContext.UserPublicationRoles.AddAsync(userPublicationRole);
                await contentDbContext.SaveChangesAsync();
            }

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUserAsync(topic.Id, user.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser_PublicationHasNoReleases()
        {
            var user = new User();
            var topic = new Topic();

            // Check publications granted via the owner role are still returned when they contain no releases
            // Set up publications without any releases, one for this topic and one for a different topic

            var userPublicationRoles = new List<UserPublicationRole>
            {
                new UserPublicationRole
                {
                    Publication = new Publication
                    {
                        Title = "Related publication 1",
                        Releases = new List<Release>(),
                        Topic = topic
                    },
                    User = user,
                    Role = Owner
                },
                new UserPublicationRole
                {
                    Publication = new Publication
                    {
                        Title = "Unrelated publication 1",
                        Releases = new List<Release>(),
                        Topic = new Topic()
                    },
                    User = user,
                    Role = Owner
                }
            };

            var contextId = Guid.NewGuid().ToString();

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUserAsync(topic.Id, user.Id);

                // Result should contain the publication related to this topic
                Assert.Single(result);
                Assert.Equal("Related publication 1", result[0].Title);
            }
        }

        [Fact]
        public async Task ReleasesCorrectlyOrdered()
        {
            var topicId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(new Topic
                {
                    Id = topicId,
                    Publications = new List<Publication>
                    {
                        new Publication
                        {
                            Id = Guid.NewGuid(),
                            Title = "Publication",
                            TopicId = topicId,
                            Releases = new List<Release>
                            {
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week1,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week11,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week3,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week2,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2001",
                                    TimePeriodCoverage = Week1,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "1999",
                                    TimePeriodCoverage = Week1,
                                    Published = DateTime.UtcNow
                                }
                            }
                        }
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = SetupPublicationRepository(context);
                var publications = await publicationService.GetAllPublicationsForTopicAsync(topicId);
                var releases = publications.Single().Releases;
                Assert.Equal("Week 1 2001", releases[0].Title);
                Assert.Equal("Week 11 2000", releases[1].Title);
                Assert.Equal("Week 3 2000", releases[2].Title);
                Assert.Equal("Week 2 2000", releases[3].Title);
                Assert.Equal("Week 1 2000", releases[4].Title);
                Assert.Equal("Week 1 1999", releases[5].Title);
            }
        }

        [Fact]
        public async Task LatestReleaseCorrectlyReportedInPublication()
        {
            var latestReleaseId = Guid.NewGuid();
            var notLatestReleaseId = Guid.NewGuid();
            var topicId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(new Topic
                {
                    Id = topicId,
                    Publications = new List<Publication>
                    {
                        new Publication
                        {
                            Id = Guid.NewGuid(),
                            Title = "Publication",
                            TopicId = topicId,
                            Releases = new List<Release>
                            {
                                new Release
                                {
                                    Id = notLatestReleaseId,
                                    ReleaseName = "2019",
                                    TimePeriodCoverage = December,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = latestReleaseId,
                                    ReleaseName = "2020",
                                    TimePeriodCoverage = June,
                                    Published = DateTime.UtcNow
                                }
                            }
                        }
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = SetupPublicationRepository(context);

                // Method under test - this return a list of publication for a user. The releases in the publication
                // should correctly report whether they are the latest or not. Note that this is dependent on the mapper
                // that we are passing in.
                var publications = await publicationService.GetAllPublicationsForTopicAsync(topicId);
                var releases = publications.Single().Releases;
                Assert.True(releases.Exists(r => r.Id == latestReleaseId && r.LatestRelease));
                Assert.True(releases.Exists(r => r.Id == notLatestReleaseId && !r.LatestRelease));
            }
        }
        
        private static PublicationRepository SetupPublicationRepository(ContentDbContext contentDbContext)
        {
            return new PublicationRepository(contentDbContext, AdminMapper());
        }
    }
}
