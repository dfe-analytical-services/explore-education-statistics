#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    [Collection(BlobCacheServiceTests)]
    public class ThemeServiceTests : BlobCacheServiceTestFixture
    {
        // TODO - this constructor sets up default expectations for the caching service that are called by each test
        // method within this test class.  In future it would be good to do targeted testing around caching within
        // each method under test.  The reason this test class needs to be identified as part of the
        // "BlobCacheServiceTests" collection is that otherwise these test methods will fail with unmatched CacheService
        // setups if run in parallel with another test class that is of the "BlobCacheServiceTests" collection.
        // This would happen because the other test class enables caching and sets a static CacheService mock at the
        // same time as this class is running.
        public ThemeServiceTests()
        {
            CacheService
                .Setup(s => s.GetItem(
                    It.IsAny<PublicationTreeCacheKey>(), typeof(IList<ThemeTree<PublicationTreeNode>>)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<PublicationTreeCacheKey>(), It.IsAny<List<ThemeTree<PublicationTreeNode>>>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetPublicationTree_Empty()
        {
            var contextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contextId);
            var service = BuildThemeService(context);

            var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
            var publicationTree = result.AssertRight();

            Assert.Empty(publicationTree);
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
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

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var releaseFileB = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFileA, releaseFileB);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

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
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);

                var topicBPublications = publicationTree[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal(PublicationType.Legacy, topicCPublications[0].Type);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FiltersThemesWithNoTopicsOrPublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
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
                            Publications = ListOf(publicationA),
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

            var releaseFile = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddAsync(releaseFile);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

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
        public async Task GetPublicationTree_FiltersThemesWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationD = new Publication
            {
                Title = "Publication D",
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
                            Publications = ListOf(publicationA)
                        },
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
                            Publications = ListOf(publicationB)
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
                            Title = "Topic C",
                            Publications = ListOf(publicationC)
                        },
                    },
                },
                new()
                {
                    Title = "Theme D",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Title = "Topic D",
                            Publications = ListOf(publicationD)
                        },
                    },
                }
            };

            var releaseFileB = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };
            var releaseD = new Release
            {
                Publication = publicationD,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
                // Not published
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFileB, releaseD);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Equal(2, publicationTree.Count);
                Assert.Equal("Theme B", publicationTree[0].Title);
                Assert.Equal("Theme C", publicationTree[1].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic B", publicationTree[0].Topics[0].Title);

                Assert.Single(publicationTree[1].Topics);
                Assert.Equal("Topic C", publicationTree[1].Topics[0].Title);

                var topicBPublications = publicationTree[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = publicationTree[1].Topics[0].Publications;

                // Publication has a legacy URL, hence it is visible
                Assert.Single(topicCPublications);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal(PublicationType.Legacy, topicCPublications[0].Type);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_FiltersTopicsWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationD = new Publication
            {
                Title = "Publication D",
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
                            Publications = ListOf(publicationA)
                        },
                        new()
                        {
                            Title = "Topic B",
                            Publications = ListOf(publicationB),
                        },
                        new()
                        {
                            Title = "Topic C",
                            Publications = ListOf(publicationC)
                        },
                        new()
                        {
                            Title = "Topic D",
                            Publications = ListOf(publicationD)
                        },
                    }
                }
            };

            var releaseFileB = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var releaseD = new Release
            {
                // Not published
                Publication = publicationD,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFileB, releaseD);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Equal(2, publicationTree[0].Topics.Count);
                Assert.Equal("Topic B", publicationTree[0].Topics[0].Title);
                Assert.Equal("Topic C", publicationTree[0].Topics[1].Title);

                var topicBPublications = publicationTree[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = publicationTree[0].Topics[1].Publications;

                // Publication has a legacy URL, hence it is visible
                Assert.Single(topicCPublications);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal(PublicationType.Legacy, topicCPublications[0].Type);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_PublicationsWithNoVisibleReleases()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C"
            };
            var publicationD = new Publication
            {
                Title = "Publication D"
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
                            publicationA,
                            publicationB,
                            publicationC,
                            publicationD
                        }
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new ReleaseFile
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File { Type = FileType.Data },
                },
                new ReleaseFile
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new File { Type = FileType.Data },
                },
            };

            var release = new Release
            {
                Publication = publicationD,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
                // Not published
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, release);
                await context.AddRangeAsync(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal("Publication B", publications[1].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_LegacyPublicationUrl()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
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
                        Publications = ListOf(publicationB, publicationC, publicationA)
                    },
                },
            };

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File
                {
                    Type = FileType.Data,
                }
            };
            var releaseFileB = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File
                {
                    Type = FileType.Data,
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, releaseFileA, releaseFileB);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Equal(3, publications.Count);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[0].Type);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(publications[0].LegacyPublicationUrl);

                Assert.Equal("publication-b", publications[1].Slug);
                Assert.Equal("Publication B", publications[1].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[1].Type);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(publications[1].LegacyPublicationUrl);

                Assert.Equal("publication-c", publications[2].Slug);
                Assert.Equal("Publication C", publications[2].Title);
                Assert.Equal(PublicationType.Legacy, publications[2].Type);
                Assert.Equal("https://legacy.url/", publications[2].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_Superseded()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var supersedingPublication = new Publication
            {
                Releases = ListOf(new Release { Published = DateTime.UtcNow }),
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                SupersededBy = supersedingPublication,
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
                        Publications = ListOf(publicationB, publicationA)
                    },
                },
            };

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };
            var releaseFileB = new ReleaseFile
            {
                Release = new()
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, releaseFileA, releaseFileB);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;
                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_ReleaseHasNoData()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
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
                    },
                },
            };

            var release = new Release
            {
                Publication = publicationA,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal(theme.Id, publicationTree[0].Id);

                var topic = Assert.Single(publicationTree[0].Topics);
                Assert.Equal(theme.Topics[0].Id, topic.Id);

                var publication = Assert.Single(topic.Publications);
                Assert.Equal(publicationA.Id, publication.Id);
                Assert.False(publication.IsSuperseded);
                Assert.True(publication.HasLiveRelease);
                Assert.False(publication.LatestReleaseHasData);
                Assert.False(publication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_ReleaseHasData()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
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
                    },
                },
            };

            var release = new Release
            {
                Publication = publicationA,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    theme,
                    release,
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            Type = FileType.Data,
                        }
                    });
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal(theme.Id, publicationTree[0].Id);

                var topic = Assert.Single(publicationTree[0].Topics);
                Assert.Equal(theme.Topics[0].Id, topic.Id);

                var publication = Assert.Single(topic.Publications);
                Assert.Equal(publicationA.Id, publication.Id);
                Assert.False(publication.IsSuperseded);
                Assert.True(publication.HasLiveRelease);
                Assert.True(publication.LatestReleaseHasData);
                Assert.True(publication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_PreviousReleaseHasData()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
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
                    },
                },
            };

            var previousRelease = new Release
            {
                Publication = publicationA,
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var release = new Release
            {
                Publication = publicationA,
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    theme,
                    previousRelease,
                    release,
                    new ReleaseFile
                    {
                        Release = previousRelease,
                        File = new File
                        {
                            Type = FileType.Data,
                        }
                    });
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal(theme.Id, publicationTree[0].Id);

                var topic = Assert.Single(publicationTree[0].Topics);
                Assert.Equal(theme.Topics[0].Id, topic.Id);

                var publication = Assert.Single(topic.Publications);
                Assert.Equal(publicationA.Id, publication.Id);
                Assert.False(publication.IsSuperseded);
                Assert.True(publication.HasLiveRelease);
                Assert.False(publication.LatestReleaseHasData);
                Assert.True(publication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FindStatistics_FiltersPublicationsWithNoReleasesAndNoLegacyUrl()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
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
                            publicationA,
                            publicationB,
                            publicationC
                        }
                    }
                }
            };

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, releaseFileA);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[0].Type);
                Assert.Null(publications[0].LegacyPublicationUrl);

                Assert.Equal("Publication C", publications[1].Title);
                Assert.Equal(PublicationType.Legacy, publications[1].Type);
                Assert.Equal("https://legacy.url/", publications[1].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_PublicationWithMultipleReleasesHasCorrectLatestReleaseType()
        {
            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Theme A summary",
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
                                Slug = "publication-a",
                            }
                        }
                    },
                },
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new ReleaseFile
                {
                    // Previous release
                    Release = new()
                    {
                        Publication = theme.Topics[0].Publications[0],
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File { Type = FileType.Data },
                },
                new ReleaseFile
                {
                    // Latest release
                    Release = new Release
                    {
                        Publication = theme.Topics[0].Publications[0],
                        ReleaseName = "2021",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2021, 1, 1),
                    },
                    File = new File { Type = FileType.Data },
                },
                new ReleaseFile
                {
                    // Not published
                    Release = new Release
                    {
                        Publication = theme.Topics[0].Publications[0],
                        ReleaseName = "2022",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.AdHocStatistics,
                    },
                    File = new File { Type = FileType.Data },
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[0].Type);
            }
        }

        [Fact]
        public async Task GetPublicationTree_PublicationWithAmendedReleaseHasCorrectLatestReleaseType()
        {
            var theme = new Theme
            {
                Title = "Theme A",
                Summary = "Theme A summary",
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
                                Slug = "publication-a",
                            }
                        }
                    },
                },
            };

            var previousRelease = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = theme.Topics[0].Publications[0],
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var latestReleaseOriginalVersion = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = theme.Topics[0].Publications[0],
                    ReleaseName = "2021",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.ExperimentalStatistics,
                    Published = new DateTime(2021, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var latestReleaseAmendedVersion = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = theme.Topics[0].Publications[0],
                    ReleaseName = "2021",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2021, 2, 1),
                    PreviousVersion = latestReleaseOriginalVersion.Release,
                },
                File = new File { Type = FileType.Data },
            };

            var unpublishedRelease = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = theme.Topics[0].Publications[0],
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.AdHocStatistics,
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme,
                    unpublishedRelease,
                    latestReleaseAmendedVersion,
                    latestReleaseOriginalVersion,
                    previousRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[0].Type);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_LatestReleaseHasDataAndIsNotSuperseded()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Releases = new List<Release> { new() },
            };
            var supersedingPublication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        Published = DateTime.UtcNow,
                    }
                }
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                SupersededBy = supersedingPublication,
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
                        Publications = ListOf(publicationB, publicationA)
                    },
                },
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(
                    PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                var publication = Assert.Single(publications);
                Assert.Equal(publicationA.Id, publication.Id);
                Assert.Equal("publication-a", publication.Slug);
                Assert.Equal("Publication A", publication.Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publication.Type);
                Assert.Null(publication.LegacyPublicationUrl);
                Assert.False(publication.IsSuperseded);
                Assert.True(publication.HasLiveRelease);
                Assert.True(publication.LatestReleaseHasData);
                Assert.True(publication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
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

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationC,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.AdHocStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

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
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);

                var topicBPublications = publicationTree[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal(PublicationType.AdHoc, topicCPublications[0].Type);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_FiltersThemesWithNoTopicsOrPublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
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
                            Publications = ListOf(publicationA),
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

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

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
        public async Task GetPublicationTree_DataTables_FiltersThemesWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
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
                            Publications = ListOf(publicationA)
                        },
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
                            Publications = ListOf(publicationB)
                        },
                    },
                },
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with latest data
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

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
        public async Task GetPublicationTree_DataTables_FiltersTopicsWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
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
                            Publications = ListOf(publicationA)
                        },
                        new()
                        {
                            Title = "Topic B",
                            Publications = ListOf(publicationB),
                        },
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with latest data
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicAPublications);
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.Equal("Publication A", topicAPublications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_FiltersPublicationsWithNoVisibleReleases()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C"
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
                            publicationA,
                            publicationB,
                            publicationC
                        }
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with latest data
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Published without latest data
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationC,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.AdHocStatistics,
                    },
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_PreviousReleaseHasData()
        {
            var publication = new Publication
            {
                Title = "Publication A",
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
                            publication,
                        }
                    }
                }
            };

            var olderRelease = new Release
            {
                Publication = publication,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };
            var latestRelease = new Release
            {
                Publication = publication,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2021, 1, 1),
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Previous release has data, but
                // not the latest release.
                new()
                {
                    Release = olderRelease,
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = latestRelease,
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                await context.AddRangeAsync(theme);
                await context.AddRangeAsync(releaseFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();
                Assert.Empty(publicationTree);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataTables_Superseded()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var supersedingPublication = new Publication
            {
                Releases = ListOf(new Release { Published = DateTime.UtcNow }),
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                SupersededBy = supersedingPublication,
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

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };
            var releaseFileB = new ReleaseFile
            {
                Release = new()
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, releaseFileA, releaseFileB);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataTables);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;
                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
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
                            Title = "Topic B",
                            Slug = "topic-B",
                            Publications = ListOf(publicationB)
                        },
                    },
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Equal(2, publicationTree.Count);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Equal("Theme B", publicationTree[1].Title);
                Assert.Equal("Theme B summary", publicationTree[1].Summary);

                Assert.Single(publicationTree[1].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                Assert.Single(publicationTree[1].Topics);
                Assert.Equal("Topic B", publicationTree[1].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("publication-a", topicAPublications[0].Slug);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);

                var topicBPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_FiltersThemesWithNoTopicsOrPublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
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
                            Publications = ListOf(publicationA),
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
                            Title = "Topic B",
                        }
                    }
                }
            };

            var releaseFile = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File
                {
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFile);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

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
        public async Task GetPublicationTree_DataCatalogue_FiltersThemesWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
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
                            Publications = ListOf(publicationA)
                        },
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
                            Publications = ListOf(publicationB)
                        },
                    },
                },
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with data - visible
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_FiltersTopicsWithNoVisiblePublications()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
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
                            Publications = ListOf(publicationA)
                        },
                        new()
                        {
                            Title = "Topic B",
                            Publications = ListOf(publicationB),
                        },
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with data - visible
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var topicAPublications = publicationTree[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_FiltersPublicationsWithNoVisibleReleases()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
            };
            var publicationC = new Publication
            {
                Title = "Publication C"
            };

            var theme = new Theme
            {
                Title = "Theme A",
                Topics = new List<Topic>
                {
                    new()
                    {
                        Title = "Topic A",
                        Publications = ListOf(publicationA, publicationB, publicationC),
                    }
                }
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Published with data - visible
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                // Published but with no data
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Not published
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationC,
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.AdHocStatistics,
                    },
                    File = new File
                    {
                        Type = FileType.Data
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_PreviousReleaseHasData()
        {
            var publication = new Publication
            {
                Title = "Publication A",
                Releases = new List<Release>
                {
                    new()
                    {
                        ReleaseName = "2021",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2021, 1, 1),
                    },
                    new()
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.OfficialStatistics,
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
                    Release = publication.Releases[0],
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                var resultPublication = Assert.Single(publications);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.False(resultPublication.IsSuperseded);
                Assert.True(resultPublication.HasLiveRelease);
                Assert.False(resultPublication.LatestReleaseHasData);
                Assert.True(resultPublication.AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_FiltersPublicationWithNoDataOnAmendedLatestRelease()
        {
            var originalRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };
            var amendedRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = new DateTime(2021, 1, 1),
                Type = ReleaseType.OfficialStatistics,
                PreviousVersion = originalRelease
            };

            var publication = new Publication
            {
                Title = "Publication A",
                Releases = ListOf(originalRelease, amendedRelease),
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
                // Amendment does not have data
                new()
                {
                    Release = amendedRelease,
                    File = new File
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Original release has data
                new()
                {
                    Release = originalRelease,
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Empty(publicationTree);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_LatestReleaseHasData()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
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

            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.OfficialStatistics,
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new File
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = new Release
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Type = ReleaseType.NationalStatistics,
                        Published = new DateTime(2020, 2, 1),
                    },
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
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

                Assert.Single(publicationTree);
                Assert.Equal("Theme A", publicationTree[0].Title);
                Assert.Equal("Theme A summary", publicationTree[0].Summary);

                Assert.Single(publicationTree[0].Topics);
                Assert.Equal("Topic A", publicationTree[0].Topics[0].Title);

                var publications = publicationTree[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);

                Assert.Equal(publicationA.Id, publications[0].Id);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[0].Type);
                Assert.Null(publications[0].LegacyPublicationUrl);
                Assert.False(publications[0].IsSuperseded);
                Assert.True(publications[0].HasLiveRelease);
                Assert.True(publications[0].LatestReleaseHasData);
                Assert.True(publications[0].AnyLiveReleaseHasData);

                Assert.Equal(publicationA.Id, publications[0].Id);
                Assert.Equal("publication-b", publications[1].Slug);
                Assert.Equal("Publication B", publications[1].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, publications[1].Type);
                Assert.Null(publications[1].LegacyPublicationUrl);
                Assert.False(publications[1].IsSuperseded);
                Assert.True(publications[1].HasLiveRelease);
                Assert.True(publications[1].LatestReleaseHasData);
                Assert.True(publications[1].AnyLiveReleaseHasData);
            }
        }

        [Fact]
        public async Task GetPublicationTree_DataCatalogue_Superseded()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
            };
            var supersedingPublication = new Publication
            {
                Releases = ListOf(new Release { Published = DateTime.UtcNow }),
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                SupersededBy = supersedingPublication,
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

            var releaseFileA = new ReleaseFile
            {
                Release = new Release
                {
                    Publication = publicationA,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.OfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                },
                File = new File { Type = FileType.Data },
            };
            var releaseFileB = new ReleaseFile
            {
                Release = new()
                {
                    Publication = publicationB,
                    ReleaseName = "2020",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Type = ReleaseType.NationalStatistics,
                    Published = new DateTime(2020, 2, 1),
                },
                File = new File { Type = FileType.Data },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(theme, releaseFileA, releaseFileB);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);
                var publicationTree = result.AssertRight();

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

        private static ThemeService BuildThemeService(
            ContentDbContext contentDbContext)
        {
            return new ThemeService(contentDbContext);
        }
    }
}
