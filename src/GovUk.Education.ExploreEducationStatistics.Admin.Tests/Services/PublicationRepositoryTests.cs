#nullable enable
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
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
                new()
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
                new()
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
                new()
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

            // Set up a publication and releases related to the topic that will be granted via the Publication
            // Owner role

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 2",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ReleaseName = "2015",
                            TimePeriodCoverage = AcademicYear
                        },
                        new()
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

            // Set up a publication and releases related to the topic that will be granted via the Publication
            // ReleaseApprover role

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Related publication 3",
                    Releases = new List<Release>
                    {
                        new()
                        {
                            ReleaseName = "2015",
                            TimePeriodCoverage = AcademicYear
                        },
                        new()
                        {
                            ReleaseName = "2016",
                            TimePeriodCoverage = AcademicYear
                        }
                    },
                    Topic = topic
                },
                User = user,
                Role = ReleaseApprover
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
                        Title = "Related publication 4",
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

            // Set up publication and release unrelated to the topic that will be granted via the Publication
            // Owner role

            userPublicationRoles.Add(new UserPublicationRole
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
                Role = Owner
            });

            // Set up publication and release unrelated to the topic that will be granted via the Publication
            // ReleaseApprover role

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = new Publication
                {
                    Title = "Unrelated publication 3",
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
                Role = ReleaseApprover
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
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

                // Result should contain Related publication 1, Related publication 2 and Related publication 3.
                // Related publication 4 is excluded because it's only granted via the PrereleaseViewer release role
                // Unrelated publications are excluded since they are for different topics
                Assert.Equal(3, result.Count);

                Assert.Equal("Related publication 2", result[0].Title);
                Assert.Equal(2, result[0].Releases.Count);
                Assert.Equal("Academic Year 2015/16", result[0].Releases[0].Title);
                Assert.Equal("Academic Year 2016/17", result[0].Releases[1].Title);

                Assert.Equal("Related publication 3", result[1].Title);
                Assert.Equal(2, result[1].Releases.Count);
                Assert.Equal("Academic Year 2015/16", result[1].Releases[0].Title);
                Assert.Equal("Academic Year 2016/17", result[1].Releases[1].Title);

                Assert.Equal("Related publication 1", result[2].Title);
                Assert.Equal(2, result[2].Releases.Count);
                Assert.Equal("Academic Year 2011/12", result[2].Releases[0].Title);
                Assert.Equal("Academic Year 2012/13", result[2].Releases[1].Title);
            }
        }

        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser_MethodologiesReturned()
        {
            var user = new User();
            var topic = new Topic();

            var methodology1Id = Guid.NewGuid();
            var methodology2Id = Guid.NewGuid();
            var methodology3Version0Id = Guid.NewGuid();
            var methodology3Version1Id = Guid.NewGuid();

            // Set up a publication related to the topic granted via a publication role
            // Include a mix of owned and adopted methodologies that are deliberately not in any title order
            var userPublicationRoles = new List<UserPublicationRole>
            {
                new()
                {
                    Publication = new Publication
                    {
                        Title = "Related publication",
                        Methodologies = new List<PublicationMethodology>
                        {
                            new()
                            {
                                Owner = false,
                                Methodology = new Methodology
                                {
                                    Versions = new List<MethodologyVersion>
                                    {
                                        new()
                                        {
                                            Id = methodology2Id,
                                            AlternativeTitle = "Methodology 2",
                                            Version = 0,
                                            Status = Draft
                                        }
                                    }
                                }
                            },                          
                            new()
                            {
                                Owner = true,
                                Methodology = new Methodology
                                {
                                    Versions = new List<MethodologyVersion>
                                    {
                                        new()
                                        {
                                            Id = methodology1Id,
                                            AlternativeTitle = "Methodology 1",
                                            Version = 0,
                                            Status = Draft
                                        }
                                    }
                                }
                            },
                            new()
                            {
                                Owner = false,
                                Methodology = new Methodology
                                {
                                    Versions = new List<MethodologyVersion>
                                    {
                                        new()
                                        {
                                            Id = methodology3Version0Id,
                                            AlternativeTitle = "Methodology 3 Version 0",
                                            Version = 0,
                                            Status = Approved
                                        },
                                        new()
                                        {
                                            Id = methodology3Version1Id,
                                            AlternativeTitle = "Methodology 3 Version 1",
                                            Version = 1,
                                            Status = Approved,
                                            PreviousVersionId = methodology3Version0Id
                                        }
                                    }
                                }
                            }
                        },
                        Topic = topic
                    },
                    User = user,
                    Role = Owner
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

                var publication = Assert.Single(result);
                Assert.NotNull(publication);
                Assert.Equal(3, publication.Methodologies.Count);

                var methodology2Version0 = Assert.Single(publication.Methodologies[0].Methodology.Versions);
                Assert.Equal(methodology2Id, methodology2Version0.Id);

                var methodology1Version0 = Assert.Single(publication.Methodologies[1].Methodology.Versions);
                Assert.Equal(methodology1Id, methodology1Version0.Id);

                Assert.Equal(2, publication.Methodologies[2].Methodology.Versions.Count);
                var methodology3Version0 = publication.Methodologies[2].Methodology.Versions[0];
                var methodology3Version1 = publication.Methodologies[2].Methodology.Versions[1];
                Assert.Equal(methodology3Version0Id, methodology3Version0.Id);
                Assert.Equal(methodology3Version1Id, methodology3Version1.Id);
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
                        new()
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
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

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
                        new()
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
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

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
                new()
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
                new()
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.Topics.AddAsync(topic);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(userPublicationRoles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupPublicationRepository(contentDbContext);
                var result = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

                // Result should contain the publication related to this topic
                var publication = Assert.Single(result);
                Assert.Equal("Related publication 1", publication.Title);
                Assert.Empty(publication.Releases);
            }
        }

        [Fact]
        public async Task GetPublicationsForTopicRelatedToUser_PublicationGrantedByBothPublicationAndReleaseRoles()
        {
            var user = new User();
            var topic = new Topic();

            // Check a Publication granted via the owner role is only returned once where it has a Release
            // also granted with roles to the same user
            // Set up a publication and releases related to the topic that will be granted via different roles

            var userReleaseRoles = new List<UserReleaseRole>();
            var userPublicationRoles = new List<UserPublicationRole>();

            var publication = new Publication
            {
                Title = "Publication",
                Topic = topic
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
                    Role = Contributor
                },
                new()
                {
                    Release = release,
                    User = user,
                    Role = Lead
                }
            });

            userPublicationRoles.Add(new UserPublicationRole
            {
                Publication = publication,
                User = user,
                Role = Owner
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
                var service = SetupPublicationRepository(contentDbContext);
                var publications = await service.GetPublicationsForTopicRelatedToUser(topic.Id, user.Id);

                var resultPublication = Assert.Single(publications);
                Assert.Equal("Publication", resultPublication.Title);

                var resultRelease = Assert.Single(resultPublication.Releases);
                Assert.Equal("Academic Year 2011/12", resultRelease.Title);
            }
        }

        [Fact]
        public async Task GetAllPublicationsForTopic_ReleasesCorrectlyOrdered()
        {
            var topicId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(new Topic
                {
                    Id = topicId,
                    Publications = new List<Publication>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Title = "Publication",
                            TopicId = topicId,
                            Releases = new List<Release>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week1,
                                    Published = DateTime.UtcNow
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week11,
                                    Published = DateTime.UtcNow
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week3,
                                    Published = DateTime.UtcNow
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = Week2,
                                    Published = DateTime.UtcNow
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2001",
                                    TimePeriodCoverage = Week1,
                                    Published = DateTime.UtcNow
                                },
                                new()
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

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = SetupPublicationRepository(context);
                var publications = await publicationService.GetAllPublicationsForTopic(topicId);
                var releases = publications.Single().Releases;

                Assert.Equal(6, releases.Count);
            }
        }

        [Fact]
        public async Task GetLatestReleaseForPublication_LiveRelease()
        {
            var expectedLatestReleaseId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();
            var pastDate = DateTime.Now.AddDays(-1);

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = AsList(
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2000",
                        TimePeriodCoverage = Week1,
                        Published = pastDate
                    },
                    // This is the latest, by year and by time period coverage
                    new Release
                    {
                        Id = expectedLatestReleaseId,
                        ReleaseName = "2002",
                        TimePeriodCoverage = Week2,
                        Published = pastDate
                    },
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2001",
                        TimePeriodCoverage = Week1,
                        Published = pastDate
                    },
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2002",
                        TimePeriodCoverage = Week1,
                        Published = pastDate
                    }
                )
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var latestRelease = await repository.GetLatestReleaseForPublication(publication.Id);
                Assert.NotNull(latestRelease);
                Assert.Equal(expectedLatestReleaseId, latestRelease!.Id);
            }
        }

        [Fact]
        public async Task GetLatestReleaseForPublication_NonLiveRelease()
        {
            var expectedLatestReleaseId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();
            var pastDate = DateTime.Now.AddDays(-1);

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = AsList(
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2000",
                        TimePeriodCoverage = Week1,
                        Published = pastDate
                    },
                    // This is the latest, by year and by time period coverage, despite it not yet having a Published
                    // date and therefore not being "Live".
                    new Release
                    {
                        Id = expectedLatestReleaseId,
                        ReleaseName = "2002",
                        TimePeriodCoverage = Week2
                    },
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2001",
                        TimePeriodCoverage = Week1,
                        Published = pastDate
                    },
                    new Release
                    {
                        Id = Guid.NewGuid(),
                        ReleaseName = "2002",
                        TimePeriodCoverage = Week1
                    }
                )
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var latestRelease = await repository.GetLatestReleaseForPublication(publication.Id);
                Assert.NotNull(latestRelease);
                Assert.Equal(expectedLatestReleaseId, latestRelease!.Id);
            }
        }

        [Fact]
        public async Task GetLatestReleaseForPublication_NoReleases()
        {
            var contextId = Guid.NewGuid().ToString();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Releases = new List<Release>()
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var latestRelease = await repository.GetLatestReleaseForPublication(publication.Id);
                Assert.Null(latestRelease);
            }
        }

        [Fact]
        public async Task GetPublicationWithAllReleases()
        {
            var release1 = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
            };
            var release2 = new Release
            {
                ReleaseName = "2001",
                TimePeriodCoverage = AcademicYear,
            };
            var methodologyVersion = new MethodologyVersion
            {
                AlternativeTitle = "Methodology Alternative Title",
                Version = 0,
                Status = Draft
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Summary = "Publication summary",
                Topic = new Topic(),
                Contact = new Contact
                {
                    ContactName = "Contact name",
                },
                Releases = new List<Release>
                {
                    release1, release2,
                },
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Owner = false,
                        Methodology = new Methodology
                        {
                            Versions = new List<MethodologyVersion>
                            {
                                methodologyVersion,
                            }
                        }
                    },
                }
            };

            var otherUnseenPublication = new Publication
            {
                Topic = new Topic(),
                Releases = new List<Release>
                {
                    new Release()
                },
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Owner = false,
                        Methodology = new Methodology
                        {
                            Versions = new List<MethodologyVersion>
                            {
                                new MethodologyVersion(),
                            }
                        }
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, otherUnseenPublication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var resultPublication = await repository.GetPublicationWithAllReleases(publication.Id);

                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Summary, resultPublication.Summary);
                Assert.Equal(publication.TopicId, resultPublication.TopicId);
                Assert.Equal(publication.Contact.ContactName, resultPublication.Contact.ContactName);

                Assert.Equal(2, resultPublication.Releases.Count);

                Assert.Single(resultPublication.Methodologies);
                Assert.Equal(publication.Methodologies[0].MethodologyId, resultPublication.Methodologies[0].Methodology.Id);
                Assert.Equal(publication.Methodologies[0].Methodology.Versions[0].AlternativeTitle,
                    resultPublication.Methodologies[0].Methodology.Versions[0].AlternativeTitle);
            }
        }

        [Fact]
        public async Task GetPublicationWithAllReleases_NoReleasesNoMethodologies()
        {
            var publication = new Publication
            {
                Topic = new Topic(),
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var resultPublication = await repository.GetPublicationWithAllReleases(publication.Id);
                Assert.Equal(publication.Id, resultPublication.Id);
            }
        }

        [Fact]
        public async Task GetPublicationForUser_NoReleaseMethodology()
        {
            var userId = Guid.NewGuid();
            var release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
            };

            var publication = new Publication
            {
                Title = "Test title",
                Summary = "Test summary",
                Topic = new Topic(),
                Releases = new List<Release>
                {
                    release1,
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var repository = SetupPublicationRepository(context);
                var resultPublication = await repository.GetPublicationForUser(publication.Id, userId);

                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Summary, resultPublication.Summary);
                Assert.Equal(publication.TopicId, resultPublication.TopicId);

                Assert.Empty(resultPublication.Releases);
                Assert.Empty(resultPublication.Methodologies);
            }
        }

        [Fact]
        public async Task ListActiveReleases()
        {
            var release2000Original = new Release
            {
                ReleaseName = "2000",
                PreviousVersionId = null,
            };
            var release2000Latest = new Release
            {
                ReleaseName = "2000",
                PreviousVersion = release2000Original,
            };

            var release2001Original = new Release
            {
                ReleaseName = "2001",
                PreviousVersion = release2000Original,
            };
            var release2001Amendment = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                PreviousVersion = release2001Original,
            };
            var release2001Latest = new Release
            {
                ReleaseName = "2001",
                PreviousVersion = release2001Amendment,
            };

            var release2002Latest = new Release
            {
                ReleaseName = "2002",
                PreviousVersionId = null,
            };
            var publication = new Publication
            {
                Releases =
                {
                    release2000Original,
                    release2000Latest,
                    release2001Original,
                    release2001Amendment,
                    release2001Latest,
                    release2002Latest,
                }
            };
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = new PublicationRepository(context, AdminMapper());
                var latestReleases = await service.ListActiveReleases(publication.Id);

                Assert.Equal(3, latestReleases.Count);

                Assert.Equal(release2000Latest.Id, latestReleases[0].Id);
                Assert.Equal(release2001Latest.Id, latestReleases[1].Id);
                Assert.Equal(release2002Latest.Id, latestReleases[2].Id);
            }
        }

        private static PublicationRepository SetupPublicationRepository(ContentDbContext contentDbContext)
        {
            return new(contentDbContext, AdminMapper());
        }
    }
}
