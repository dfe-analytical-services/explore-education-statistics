#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ThemeServiceTests
    {
        [Fact]
        public async Task GetPublicationTree_NoThemes()
        {
            var contextId = Guid.NewGuid().ToString();
            await using var context = InMemoryContentDbContext(contextId);
            var service = BuildThemeService(context);

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
                Type = ReleaseType.OfficialStatistics,
                Published = DateTime.UtcNow,
            };
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
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
                }
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

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
                Assert.Equal(PublicationType.NationalAndOfficial, topicAPublications[0].Type);
                Assert.True(topicAPublications[0].HasLiveRelease);
                Assert.False(topicAPublications[0].LatestReleaseHasData);
                Assert.False(topicAPublications[0].AnyLiveReleaseHasData);

                var topicBPublications = publicationTree[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal(PublicationType.NationalAndOfficial, topicBPublications[0].Type);
                Assert.True(topicBPublications[0].HasLiveRelease);
                Assert.False(topicBPublications[0].LatestReleaseHasData);
                Assert.False(topicBPublications[0].AnyLiveReleaseHasData);

                var topicCPublications = publicationTree[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal(PublicationType.Legacy, topicCPublications[0].Type);
                Assert.False(topicCPublications[0].HasLiveRelease);
                Assert.False(topicCPublications[0].LatestReleaseHasData);
                Assert.False(topicCPublications[0].AnyLiveReleaseHasData);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_ThemesWithNoTopicsOrPublications_Excluded()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
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
                var service = BuildThemeService(context);

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
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = DateTime.UtcNow,
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = releaseB,
                Releases = new List<Release>
                {
                    releaseB
                }
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationD = new Publication
            {
                Title = "Publication D",
                Releases = new List<Release>
                {
                    // Not published
                    new()
                    {
                        ReleaseName = "2020",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Type = ReleaseType.NationalStatistics,
                        Published = null,
                    }
                }
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var publicationTree = await service.GetPublicationTree();

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
        public async Task GetPublicationTree_LegacyPublicationUrl()
        {
            var releaseA = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
                Published = DateTime.UtcNow,
            };
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
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
                },
                LegacyPublicationUrl = new Uri("https://legacy.url/")
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var publicationTree = await service.GetPublicationTree();

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
                Assert.Null(publications[1].LegacyPublicationUrl);

                Assert.Equal("publication-c", publications[2].Slug);
                Assert.Equal("Publication C", publications[2].Title);
                Assert.Equal(PublicationType.Legacy, publications[2].Type);
                Assert.Equal("https://legacy.url/", publications[2].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_PublicationsWithNoReleasesAndNoLegacyUrl_Excluded()
        {
            var releaseA = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = releaseA,
                Releases = new List<Release>
                {
                    releaseA
                }
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddRangeAsync(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var publicationTree = await service.GetPublicationTree();

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
            var previousRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
                Published = DateTime.UtcNow,
            };

            var unpublishedRelease = new Release
            {
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.AdHocStatistics,
                Published = null,
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
                        Publications = new List<Publication>
                        {
                            new()
                            {
                                Title = "Publication A",
                                Slug = "publication-a",
                                LatestPublishedRelease = latestRelease,
                                Releases = new List<Release>
                                {
                                    previousRelease,
                                    latestRelease,
                                    unpublishedRelease
                                }
                            }
                        }
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.Themes.AddAsync(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var publicationTree = await service.GetPublicationTree();

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
        public async Task GetPublicationTree_TopicsWithNoVisiblePublications_Excluded()
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

                var publicationTree = await service.GetPublicationTree();

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
        public async Task GetPublicationTree_LatestReleaseHasData()
        {
            var latestRelease = new Release
            {
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
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
                var service = BuildThemeService(context);

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
                Type = ReleaseType.OfficialStatistics,
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
                var service = BuildThemeService(context);

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
                Type = ReleaseType.OfficialStatistics,
                Published = DateTime.UtcNow,
            };

            var unpublishedRelease = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.OfficialStatistics,
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
                var service = BuildThemeService(context);

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
                Type = ReleaseType.OfficialStatistics,
                Published = DateTime.UtcNow,
            };
            var releaseB = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Type = ReleaseType.NationalStatistics,
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
                var service = BuildThemeService(context);

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

        private static ThemeService BuildThemeService(
            ContentDbContext contentDbContext)
        {
            return new ThemeService(
                contentDbContext: contentDbContext,
                publicationRepository: new PublicationRepository(contentDbContext));
        }
    }
}
