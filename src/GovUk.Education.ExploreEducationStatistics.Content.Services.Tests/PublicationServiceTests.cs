#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService.
    PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class PublicationServiceTests
    {
        private const string PublicationSlug = "publication-slug";

        [Fact]
        public async Task Get()
        {
            var release2000Version0Id = Guid.NewGuid();
            var release2000Version1Id = Guid.NewGuid();
            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = release2000Version1Id,
                Releases = new List<Release>
                {
                    new() // latest published release
                    {
                        Id = release2000Version1Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 1,
                        PreviousVersionId = release2000Version0Id,
                    },
                    new() // previous version
                    {
                        Id = release2000Version0Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                    new() // not published
                    {
                        ReleaseName = "2001",
                        Slug = "2001",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = null,
                        Version = 0,
                    },
                    new() // published so appears in ListPublishedReleases result
                    {
                        ReleaseName = "1999",
                        Slug = "1999",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Legacy release description",
                        Url = "https://legacy.release.com",
                        Order = 0,
                    }
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                        Summary = "Test theme summary"
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "External methodology title",
                    Url = "https://external.methodology.com",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.Equal(publication.Title, publicationViewModel.Title);
                Assert.Equal(publication.Slug, publicationViewModel.Slug);
                Assert.False(publicationViewModel.IsSuperseded);

                Assert.Equal(2, publicationViewModel.Releases.Count);
                Assert.Equal(publication.Releases[0].Id, publicationViewModel.LatestReleaseId);

                Assert.Equal(publication.Releases[0].Id, publicationViewModel.Releases[0].Id);
                Assert.Equal(publication.Releases[0].Slug, publicationViewModel.Releases[0].Slug);
                Assert.Equal(publication.Releases[0].Title, publicationViewModel.Releases[0].Title);

                Assert.Equal(publication.Releases[3].Id, publicationViewModel.Releases[1].Id);
                Assert.Equal(publication.Releases[3].Slug, publicationViewModel.Releases[1].Slug);
                Assert.Equal(publication.Releases[3].Title, publicationViewModel.Releases[1].Title);

                Assert.Single(publication.LegacyReleases);
                Assert.Equal(publication.LegacyReleases[0].Id, publicationViewModel.LegacyReleases[0].Id);
                Assert.Equal(publication.LegacyReleases[0].Description,
                    publicationViewModel.LegacyReleases[0].Description);
                Assert.Equal(publication.LegacyReleases[0].Url, publicationViewModel.LegacyReleases[0].Url);

                Assert.Equal(publication.Topic.Theme.Id, publicationViewModel.Topic.Theme.Id);
                Assert.Equal(publication.Topic.Theme.Slug, publicationViewModel.Topic.Theme.Slug);
                Assert.Equal(publication.Topic.Theme.Title, publicationViewModel.Topic.Theme.Title);
                Assert.Equal(publication.Topic.Theme.Summary, publicationViewModel.Topic.Theme.Summary);

                Assert.Equal(publication.Contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(publication.Contact.TeamEmail, publicationViewModel.Contact.TeamEmail);
                Assert.Equal(publication.Contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);

                Assert.NotNull(publicationViewModel.ExternalMethodology);
                Assert.Equal(publication.ExternalMethodology.Title, publicationViewModel.ExternalMethodology!.Title);
                Assert.Equal(publication.ExternalMethodology.Url, publicationViewModel.ExternalMethodology.Url);
            }
        }

        [Fact]
        public async Task Get_IsSuperseded_SupersedingPublicationHasPublishedRelease()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = releaseId,
                SupersededBy = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Superseding Publication Title",
                    Slug = "superseding-publication",
                    LatestPublishedReleaseId = Guid.NewGuid()
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow
                    },
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.True(publicationViewModel.IsSuperseded);
                
                Assert.NotNull(publicationViewModel.SupersededBy);
                Assert.Equal(publication.SupersededBy.Id, publicationViewModel.SupersededBy!.Id);
                Assert.Equal(publication.SupersededBy.Title, publicationViewModel.SupersededBy.Title);
                Assert.Equal(publication.SupersededBy.Slug, publicationViewModel.SupersededBy.Slug);
            }
        }

        [Fact]
        public async Task Get_IsSuperseded_SupersedingPublicationHasNoPublishedRelease()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = releaseId,
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = null
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear
                    }
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.False(publicationViewModel.IsSuperseded);
                Assert.Null(publicationViewModel.SupersededBy);
            }
        }

        [Fact]
        public async Task Get_PublicationHasNoPublishedRelease()
        {
            var publication = new Publication
            {
                Slug = PublicationSlug,
                Releases = new List<Release>
                {
                    new()
                    {
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = null
                    }
                },
                LatestPublishedRelease = null
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(PublicationSlug);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Get_NoPublication()
        {
            var service = SetupPublicationService();

            var result = await service.Get("nonexistent-publication");

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetPublicationTree_NoThemes()
        {
            var contextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contextId);
            var service = SetupPublicationService(context);

            var publicationTree = await service.GetPublicationTree();
            Assert.Empty(publicationTree);
        }

        [Fact]
        public async Task GetPublicationTree_MultipleThemesTopics()
        {
            var releaseA = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };
            var releaseC = new Release
            {
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LatestPublishedRelease = releaseA,
                Releases = new List<Release>
                {
                    releaseA
                }
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                LatestPublishedRelease = releaseB,
                Releases = new List<Release>
                {
                    releaseB
                }
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                LatestPublishedRelease = releaseC,
                Releases = new List<Release>
                {
                    releaseC
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme A",
                    Summary = "Theme A summary",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic A",
                            Publications = ListOf(publicationA)
                        },
                        new()
                        {
                            Title = "Topic B",
                            Publications = ListOf(publicationB)
                        }
                    },
                },
                new()
                {
                    Title = "Theme B",
                    Summary = "Theme B summary",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic C",
                            Slug = "topic-C",
                            Publications = ListOf(publicationC)
                        },
                    },
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Equal(2, publicationTree.Count);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Equal("Theme B", publicationTree[1].Title);
                Assert.Equal("Theme B summary", publicationTree[1].Summary);

                Assert.Equal(2, publicationTree[0].Topics.Count);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);
                Assert.Equal("Topic B", publicationTree[0].Topics[1].Title);

                Assert.Single(publicationTree[1].Topics);
                Assert.Equal("Topic C", publicationTree[1].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("publication-a", topicAPublications[0].Slug);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.False(topicAPublications[0].LatestReleaseHasData);
                Assert.False(topicAPublications[0].AnyLiveReleaseHasData);

                var topicBPublications = publicationTree[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.False(topicBPublications[0].LatestReleaseHasData);
                Assert.False(topicBPublications[0].AnyLiveReleaseHasData);

                var topicCPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.False(topicCPublications[0].LatestReleaseHasData);
                Assert.False(topicCPublications[0].AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_ThemesWithNoTopicsOrPublications_Excluded()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme A",
                },
                new()
                {
                    Title = "Theme B",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic A",
                            Publications = new List<Publication>
                            {
                                new()
                                {
                                    Title = "Publication A",
                                    LatestPublishedRelease = release,
                                    Releases = new List<Release>
                                    {
                                        release
                                    }
                                }
                            }
                        },
                    },
                },
                new()
                {
                    Title = "Theme C",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic B"
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Single(publicationTree);
                Assert.Equal("Theme B", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_ThemesWithNoVisiblePublications_Excluded()
        {
            var releaseA = new Release
            {
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var releaseB = new Release
            {
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme A",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic A",
                            Publications = new List<Publication>
                            {
                                // Publication has a published release
                                new()
                                {
                                    Title = "Publication A",
                                    LatestPublishedRelease = releaseB,
                                    Releases = new List<Release>
                                    {
                                        releaseB
                                    }
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Theme B",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic B",
                            Publications = new List<Publication>
                            {
                                // Publication has a published and an unpublished release
                                new()
                                {
                                    Title = "Publication B",
                                    LatestPublishedRelease = releaseA,
                                    Releases = new List<Release>
                                    {
                                        releaseA,
                                        new()
                                        {
                                            ReleaseName = "2021",
                                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                                            Published = null
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Theme C",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic C",
                            Publications = new List<Publication>
                            {
                                // Publication has no releases
                                new()
                                {
                                    Title = "Publication C",
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Theme D",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic D",
                            Publications = new List<Publication>
                            {
                                // Publication has no published releases
                                new()
                                {
                                    Title = "Publication D",
                                    LatestPublishedRelease = null,
                                    Releases = new List<Release>
                                    {
                                        new()
                                        {
                                            ReleaseName = "2022",
                                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                                            Published = null
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Releases.AddRangeAsync(releaseA, releaseB);
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                // Theme A / Topic A should be included as Publication A has a published release
                // Theme B / Topic B should be included as Publication B has a published and unpublished releases
                // Theme C / Topic C should be excluded as Publication C has no releases
                // Theme D / Topic D should be excluded as Publication D only has an unpublished release

                Assert.Equal(2, publicationTree.Count);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme B", publicationTree[1].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);

                Assert.Single(publicationTree[1].Topics);
                Assert.Equal("Topic B", publicationTree[1].Topics[0].Title);

                var topicBPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_PublicationsWithNoReleases_Excluded()
        {
            var releaseA = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        Publications = new List<Publication>
                        {
                            // Publication has a published release
                            new()
                            {
                                Title = "Publication A",
                                LatestPublishedRelease = releaseA,
                                Releases = new List<Release>
                                {
                                    releaseA
                                }
                            },
                            // Publication has no releases
                            new()
                            {
                                Title = "Publication B",
                                Releases = new List<Release>()
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_TopicsWithNoVisiblePublications_Excluded()
        {
            var releaseA = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme A",
                    Topics = new List<Topic>
                    {
                        // Topic has a publication with a published release
                        new()
                        {
                            Title = "Topic A",
                            Publications = new List<Publication>
                            {
                                // Publication has a published release
                                new()
                                {
                                    Title = "Publication A",
                                    LatestPublishedRelease = releaseA,
                                    Releases = new List<Release>
                                    {
                                        releaseA
                                    }
                                }
                            }
                        },
                        // Topic only has a publication with no published release
                        new()
                        {
                            Title = "Topic B",
                            Publications = new List<Publication>
                            {
                                new()
                                {
                                    Title = "Publication B",
                                    LatestPublishedRelease = null,
                                    Releases = new List<Release>
                                    {
                                        new()
                                        {
                                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                                            ReleaseName = "2020"
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Releases.AddAsync(releaseA);
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                // Topic A should be included as Publication A has a published release
                // Topic B should be excluded as Publication B only has an unpublished release
                
                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestReleaseHasData()
        {
            var latestRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = latestRelease,
                Releases = new List<Release>
                {
                    latestRelease,
                    new()
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Published = new DateTime(2020, 1, 1),
                    }
                }
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        Publications = ListOf(publication)
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Latest release has no data
                new()
                {
                    Release = latestRelease,
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Older release has no data
                new()
                {
                    Release = publication.Releases[1],
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                var resultPublication = Assert.Single(publications);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.True(resultPublication.LatestReleaseHasData);
                Assert.True(resultPublication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_PreviousReleaseHasData()
        {
            var latestRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = latestRelease,
                Releases = new List<Release>
                {
                    latestRelease,
                    new()
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Published = new DateTime(2020, 1, 1),
                    }
                }
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        Publications = ListOf(publication)
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Latest release has no data
                new()
                {
                    Release = latestRelease,
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Older release has data, so the publication is visible
                new()
                {
                    Release = publication.Releases[1],
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                var resultPublication = Assert.Single(publications);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.False(resultPublication.LatestReleaseHasData);
                Assert.True(resultPublication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_UnpublishedReleaseHasData()
        {
            var latestRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };

            var unpublishedRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = null,
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LatestPublishedRelease = latestRelease,
                Releases = new List<Release>
                {
                    latestRelease,
                    unpublishedRelease
                }
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Theme A summary",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        Publications = ListOf(publicationA)
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Latest release has no data
                new()
                {
                    Release = latestRelease,
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Unpublished release has data but this shouldn't alter anything
                new()
                {
                    Release = unpublishedRelease,
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(theme);
                await context.ReleaseFiles.AddRangeAsync(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Single(publicationTree);
                Assert.Equal(theme.Id, publicationTree[0].Id);

                var topic = Assert.Single(publicationTree[0].Topics);
                Assert.Equal(theme.Topics[0].Id, topic.Id);

                var publication = Assert.Single(topic.Publications);
                Assert.Equal(publicationA.Id, publication.Id);
                Assert.False(publication.LatestReleaseHasData);
                Assert.False(publication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_Superseded()
        {
            var releaseA = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow,
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LatestPublishedRelease = releaseA,
                Releases = new List<Release>
                {
                    releaseA
                }
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                LatestPublishedRelease = releaseB,
                Releases = new List<Release>
                {
                    releaseB
                },
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = Guid.NewGuid()
                }
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Theme A summary",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        // Publications are in random order
                        // to check that ordering is done by title
                        Publications = ListOf(publicationB, publicationA)
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                Assert.Equal(1, publicationTree.Count);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;
                Assert.Equal(2, publications.Count);

                Assert.Equal("Publication A", publications[0].Title);
                Assert.False(publications[0].IsSuperseded);

                Assert.Equal("Publication B", publications[1].Title);
                Assert.True(publications[1].IsSuperseded);
            }
        }
        
        [Fact]
        public async Task GetPublicationTree_SupersedingPublicationBuildsSupersededViewModel()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LatestPublishedReleaseId = releaseId,
                SupersededBy = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    Slug = "publication-b",
                    LatestPublishedReleaseId = Guid.NewGuid()
                }
            };
            
            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme A",
                    Summary = "Theme A summary",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic A",
                            Publications = ListOf(publication)
                        },
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();

                var testTopicPublications = publicationTree[0].Topics[0].Publications;
                
                var testTopicPublication = Assert.Single(testTopicPublications);
                
                Assert.True(testTopicPublication.IsSuperseded);
                
                Assert.NotNull(testTopicPublication.SupersededBy);
                Assert.Equal(publication.SupersededBy.Id, testTopicPublication.SupersededBy!.Id);
                Assert.Equal(publication.SupersededBy.Title, testTopicPublication.SupersededBy.Title);
                Assert.Equal(publication.SupersededBy.Slug, testTopicPublication.SupersededBy.Slug);
            }
        }

        [Fact]
        public async Task ListPublications()
        {
            var publicationA = new Publication
            {
                Slug = "publication-a",
                Title = "Publication A",
                Summary = "Publication A summary",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2020, 1, 1)
                }
            };

            var publicationB = new Publication
            {
                Slug = "publication-b",
                Title = "Publication B",
                Summary = "Publication B summary",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = new DateTime(2021, 1, 1)
                }
            };

            var publicationC = new Publication
            {
                Slug = "publication-c",
                Title = "Publication C",
                Summary = "Publication C summary",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = new DateTime(2022, 1, 1)
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("publication-a", results[0].Slug);
                Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);
                Assert.Equal("Publication A", results[0].Title);
                Assert.Equal("Publication A summary", results[0].Summary);
                Assert.Equal("Theme 1 title", results[0].Theme);
                Assert.Equal(NationalStatistics, results[0].Type);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("publication-b", results[1].Slug);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);
                Assert.Equal("Publication B", results[1].Title);
                Assert.Equal("Publication B summary", results[1].Summary);
                Assert.Equal("Theme 1 title", results[1].Theme);
                Assert.Equal(OfficialStatistics, results[1].Type);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal("publication-c", results[2].Slug);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
                Assert.Equal("Publication C", results[2].Title);
                Assert.Equal("Publication C summary", results[2].Summary);
                Assert.Equal("Theme 2 title", results[2].Theme);
                Assert.Equal(AdHocStatistics, results[2].Type);
            }
        }

        [Fact]
        public async Task ListPublications_ExcludesUnpublishedPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Not published (no published release)
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = null
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
            }
        }

        [Fact]
        public async Task ListPublications_ExcludesSupersededPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Published
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Not published (no published release)
            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseId = null
            };

            // Not published (superseded by publicationB which is published)
            var publicationD = new Publication
            {
                Title = "Publication D",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                },
                SupersededBy = publicationB
            };

            // Published (superseded by publicationC but it's not published yet)
            var publicationE = new Publication
            {
                Title = "Publication E",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                },
                SupersededBy = publicationC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB,
                                publicationC,
                                publicationD,
                                publicationE
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(publicationE.Id, results[2].Id);
            }
        }

        [Fact]
        public async Task ListPublications_FilterByTheme()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                new()
                                {
                                    Title = "Publication C",
                                    LatestPublishedRelease = new Release
                                    {
                                        Type = AdHocStatistics,
                                        Published = new DateTime(2022, 1, 1)
                                    }
                                }
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    themeId: themes[0].Id
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(2, results.Count);

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("Theme 1 title", results[0].Theme);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Theme 1 title", results[1].Theme);
            }
        }

        [Fact]
        public async Task ListPublications_FilterByReleaseType()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    releaseType: OfficialStatistics
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Single(results);

                Assert.Equal(publicationB.Id, results[0].Id);
                Assert.Equal(OfficialStatistics, results[0].Type);
            }
        }

        [Fact]
        public async Task ListPublications_Search_SortByRelevance_Desc()
        {
            var releaseA = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseB = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseC = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var topic = new Topic
            {
                Theme = new Theme
                {
                    Title = "Theme title"
                }
            };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = SetupPublicationService(contentDbContext.Object);

            var pagedResult = (await service.ListPublications(
                search: "term",
                sort: null, // Sort should default to relevance
                order: null // Order should default to descending
            )).AssertRight();
            var results = pagedResult.Results;

            Assert.Equal(3, results.Count);

            // Expect results sorted by relevance in descending order

            Assert.Equal(publications[2].Id, results[0].Id);
            Assert.Equal(300, results[0].Rank);

            Assert.Equal(publications[0].Id, results[1].Id);
            Assert.Equal(200, results[1].Rank);

            Assert.Equal(publications[1].Id, results[2].Id);
            Assert.Equal(100, results[2].Rank);
        }

        [Fact]
        public async Task ListPublications_Search_SortByRelevance_Asc()
        {
            var releaseA = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseB = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseC = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var topic = new Topic
            {
                Theme = new Theme
                {
                    Title = "Theme title"
                }
            };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = SetupPublicationService(contentDbContext.Object);

            var pagedResult = (await service.ListPublications(
                search: "term",
                sort: null, // Sort should default to relevance
                order: Asc
            )).AssertRight();
            var results = pagedResult.Results;

            Assert.Equal(3, results.Count);

            // Expect results sorted by relevance in ascending order

            Assert.Equal(publications[1].Id, results[0].Id);
            Assert.Equal(100, results[0].Rank);

            Assert.Equal(publications[0].Id, results[1].Id);
            Assert.Equal(200, results[1].Rank);

            Assert.Equal(publications[2].Id, results[2].Id);
            Assert.Equal(300, results[2].Rank);
        }

        [Fact]
        public async Task ListPublications_SortByPublished_Desc()
        {
            var releaseA = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2020, 1, 1)
            };

            var releaseB = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2021, 1, 1)
            };

            var releaseC = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2022, 1, 1)
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = releaseA
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = releaseB
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = releaseC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: Published,
                    order: null // Order should default to descending
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by published date in descending order

                Assert.Equal(publicationC.Id, results[0].Id);
                Assert.Equal(new DateTime(2022, 1, 1), results[0].Published);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                Assert.Equal(publicationA.Id, results[2].Id);
                Assert.Equal(new DateTime(2020, 1, 1), results[2].Published);
            }
        }

        [Fact]
        public async Task ListPublications_SortByPublished_Asc()
        {
            var releaseA = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2020, 1, 1)
            };

            var releaseB = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2021, 1, 1)
            };

            var releaseC = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2022, 1, 1)
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = releaseA
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = releaseB
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = releaseC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: Published,
                    order: Asc
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by published date in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
            }
        }

        [Fact]
        public async Task ListPublications_SortByTitle_Desc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: null, // Sort should default to title
                    order: Desc
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in descending order

                Assert.Equal(publicationC.Id, results[0].Id);
                Assert.Equal("Publication C", results[0].Title);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Publication B", results[1].Title);

                Assert.Equal(publicationA.Id, results[2].Id);
                Assert.Equal("Publication A", results[2].Title);
            }
        }

        [Fact]
        public async Task ListPublications_SortByTitle_Asc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: null, // Sort should default to title
                    order: null // Order should default to ascending
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("Publication A", results[0].Title);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Publication B", results[1].Title);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal("Publication C", results[2].Title);
            }
        }

        private static PublicationService SetupPublicationService(
            ContentDbContext? contentDbContext = null,
            IPublicationRepository? publicationRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                publicationRepository ?? new PublicationRepository(contentDbContext)
            );
        }
    }
}
