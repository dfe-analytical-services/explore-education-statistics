#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ThemeServiceTests
    {
        [Fact]
        public async Task GetPublicationTree_Empty()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationTree_SingleThemeTopic()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                Summary = "Publication C summary",
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 2, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Equal(3, publications.Count);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A summary", publications[0].Summary);
                Assert.Equal("Publication A", publications[0].Title);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(publications[0].LegacyPublicationUrl);

                Assert.Equal("publication-b", publications[1].Slug);
                Assert.Equal("Publication B summary", publications[1].Summary);
                Assert.Equal("Publication B", publications[1].Title);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(publications[1].LegacyPublicationUrl);

                Assert.Equal("publication-c", publications[2].Slug);
                Assert.Equal("Publication C summary", publications[2].Summary);
                Assert.Equal("Publication C", publications[2].Title);
                Assert.Equal("https://legacy.url/", publications[2].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
                LegacyPublicationUrl = new Uri("https://legacy.url/")
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                Summary = "Publication C summary",
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 2, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Equal(2, result.Count);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Equal("Theme B", result[1].Title);
                Assert.Equal("Theme B summary", result[1].Summary);

                Assert.Equal(2, result[0].Topics.Count);
                Assert.Equal("Topic A", result[0].Topics[0].Title);
                Assert.Equal("Topic B", result[0].Topics[1].Title);

                Assert.Single(result[1].Topics);
                Assert.Equal("Topic C", result[1].Topics[0].Title);

                var topicAPublications = result[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("publication-a", topicAPublications[0].Slug);
                Assert.Equal("Publication A summary", topicAPublications[0].Summary);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);

                var topicBPublications = result[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B summary", topicBPublications[0].Summary);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                // Publication has a legacy url but it's not set because Releases exist
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = result[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C summary", topicCPublications[0].Summary);
                Assert.Equal("Publication C", topicCPublications[0].Title);
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Single(result);
                Assert.Equal("Theme B", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

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
                            Publications =  ListOf(publicationA)
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 1, 1),
                },
                // Not published
                new()
                {
                    Publication = publicationD
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Equal(2, result.Count);
                Assert.Equal("Theme B", result[0].Title);
                Assert.Equal("Theme C", result[1].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic B", result[0].Topics[0].Title);

                Assert.Single(result[1].Topics);
                Assert.Equal("Topic C", result[1].Topics[0].Title);

                var topicBPublications = result[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = result[1].Topics[0].Publications;

                // Publication has a legacy URL, hence it is visible
                Assert.Single(topicCPublications);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FiltersTopicsWithNoVisiblePublications()
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 1, 1),
                },
                // Not published
                new()
                {
                    Publication = publicationD
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Equal(2, result[0].Topics.Count);
                Assert.Equal("Topic B", result[0].Topics[0].Title);
                Assert.Equal("Topic C", result[0].Topics[1].Title);

                var topicBPublications = result[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = result[0].Topics[1].Publications;

                // Publication has a legacy URL, hence it is visible
                Assert.Single(topicCPublications);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal("https://legacy.url/", topicCPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FiltersPublicationsWithNoVisibleReleases()
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 2, 1),
                },
                // Not published
                new()
                {
                    Publication = publicationD,
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal("Publication B", publications[1].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_FiltersPublicationsWithNoReleasesAndNoLegacyUrl()
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

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree();

                var publications = result[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Null(publications[0].LegacyPublicationUrl);

                Assert.Equal("Publication C", publications[1].Title);
                Assert.Equal("https://legacy.url/", publications[1].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_SingleThemeTopic()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                new() {
                    Release = new()
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Equal(2, publications.Count);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A summary", publications[0].Summary);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Null(publications[0].LegacyPublicationUrl);

                Assert.Equal("publication-b", publications[1].Slug);
                Assert.Equal("Publication B summary", publications[1].Summary);
                Assert.Equal("Publication B", publications[1].Title);
                Assert.Null(publications[1].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                Summary = "Publication C summary",
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                new() {
                    Release = new()
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                new() {
                    Release = new()
                    {
                        Publication = publicationC,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 2, 1),
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Equal(2, result.Count);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Equal("Theme B", result[1].Title);
                Assert.Equal("Theme B summary", result[1].Summary);

                Assert.Equal(2, result[0].Topics.Count);
                Assert.Equal("Topic A", result[0].Topics[0].Title);
                Assert.Equal("Topic B", result[0].Topics[1].Title);

                Assert.Single(result[1].Topics);
                Assert.Equal("Topic C", result[1].Topics[0].Title);

                var topicAPublications = result[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("publication-a", topicAPublications[0].Slug);
                Assert.Equal("Publication A summary", topicAPublications[0].Summary);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.Null(topicAPublications[0].LegacyPublicationUrl);

                var topicBPublications = result[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B summary", topicBPublications[0].Summary);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);

                var topicCPublications = result[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C summary", topicCPublications[0].Summary);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Null(topicBPublications[0].LegacyPublicationUrl);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_FiltersThemesWithNoTopicsOrPublications()
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Single(result);
                Assert.Equal("Theme B", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_FiltersThemesWithNoVisiblePublications()
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
                            Publications =  ListOf(publicationA)
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new() {
                    Release = new()
                    {
                        Publication = publicationB,
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var topicAPublications = result[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_FiltersTopicsWithNoVisiblePublications()
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                // Not published
                new() {
                    Release = new()
                    {
                        Publication = publicationB,
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var topicAPublications = result[0].Topics[0].Publications;

                // Publication has a published release, hence it is visible
                Assert.Single(topicAPublications);
                Assert.Equal("Publication A", topicAPublications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_FiltersPublicationsWithNoVisibleReleases()
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
                new() {
                    Release = new()
                    {
                        Publication = publicationA,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                // Published without latest data
                new() {
                    Release = new()
                    {
                        Publication = publicationB,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                        Published = new DateTime(2020, 1, 1),
                    },
                    File = new()
                    {
                        Type = FileType.Ancillary
                    }
                },
                // Not published
                new() {
                    Release = new()
                    {
                        Publication = publicationC,
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        ReleaseName = "2020",
                    },
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_DoesNotFilterPublicationWithDataOnLatestRelease()
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
                Published = new DateTime(2020, 1, 1),
            };
            var latestRelease = new Release
            {
                Publication = publication,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2021, 1, 1),
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Previous release has data, but
                // not the latest release.
                new()
                {
                    Release = olderRelease,
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = latestRelease,
                    File = new()
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

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationTree_LatestData_FiltersPublicationWithNoDataOnLatestAmendedRelease()
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

            var originalRelease = new Release
            {
                Publication = publication,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
            };
            var amendedRelease = new Release
            {
                Publication = publication,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 2, 1),
                PreviousVersion = originalRelease
            };

            var releaseFiles = new List<ReleaseFile>
            {
                // Previous version has data, but
                // not the amended release.
                new()
                {
                    Release = originalRelease,
                    File = new()
                    {
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = amendedRelease,
                    File = new()
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
                await context.AddRangeAsync(amendedRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationTree(PublicationTreeFilter.LatestData);
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_Empty()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildThemeService(context);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_SingleThemeTopic()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                Summary = "Publication C summary",
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
                        Publications = new List<Publication>
                        {
                            publicationB,
                            publicationC,
                            publicationA,
                        }
                    },
                },
            };

            var releases = new List<Release>
            {
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                    ReleaseName = "2020",
                },
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2018, 1, 1),
                    ReleaseName = "2018"
                },
                new()
                {
                    Publication = publicationA,
                    Published = new DateTime(2019, 1, 1),
                    ReleaseName = "2019"
                },
                new()
                {
                    Publication = publicationB,
                    Published = new DateTime(2020, 2, 1),
                    ReleaseName = "2020"
                },
                new()
                {
                    Publication = publicationC,
                    Published = new DateTime(2020, 3, 1),
                    ReleaseName = "2020"
                },
            };

            var publicationALatestRelease = releases[0];
            var publicationBLatestRelease = releases[3];
            var publicationCLatestRelease = releases[4];

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var publicationAFiles = new List<FileInfo>
                {
                    new()
                    {
                        Name = "Publication A file 1",
                        Type = FileType.Ancillary,
                    },
                    new()
                    {
                        Name = "Publication A file 2",
                        Type = FileType.Data,
                    },
                };
                var publicationBFiles = new List<FileInfo>
                {
                    new()
                    {
                        Name = "Publication B file 1",
                        Type = FileType.Data,
                    },
                };
                var publicationCFiles = new List<FileInfo>
                {
                    new()
                    {
                        Name = "Publication C file 1",
                        Type = FileType.Data,
                    },
                };

                var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == publicationALatestRelease.Id)))
                    .ReturnsAsync(publicationAFiles);

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == publicationBLatestRelease.Id)))
                    .ReturnsAsync(publicationBFiles);

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == publicationCLatestRelease.Id)))
                    .ReturnsAsync(publicationCFiles);

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Equal(3, publications.Count);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("Publication A summary", publications[0].Summary);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Equal("Academic Year 2018/19", publications[0].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", publications[0].LatestReleaseTime);
                Assert.Equal(publicationALatestRelease.Id, publications[0].LatestReleaseId);

                Assert.Equal(2, publications[0].DownloadFiles.Count);
                Assert.Equal(publicationAFiles[0], publications[0].DownloadFiles[0]);
                Assert.Equal(publicationAFiles[1], publications[0].DownloadFiles[1]);

                Assert.Equal("publication-b", publications[1].Slug);
                Assert.Equal("Publication B summary", publications[1].Summary);
                Assert.Equal("Publication B", publications[1].Title);
                Assert.Equal("Academic Year 2020/21", publications[1].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", publications[1].LatestReleaseTime);
                Assert.Equal(publicationBLatestRelease.Id, publications[1].LatestReleaseId);

                Assert.Single(publications[1].DownloadFiles);
                Assert.Equal(publicationBFiles[0], publications[1].DownloadFiles[0]);

                Assert.Equal("publication-c", publications[2].Slug);
                Assert.Equal("Publication C summary", publications[2].Summary);
                Assert.Equal("Publication C", publications[2].Title);
                Assert.Equal("Academic Year 2020/21", publications[2].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", publications[2].LatestReleaseTime);
                Assert.Equal(publicationCLatestRelease.Id, publications[2].LatestReleaseId);

                Assert.Single(publications[2].DownloadFiles);
                Assert.Equal(publicationCFiles[0], publications[2].DownloadFiles[0]);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_MultipleThemesTopics()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Summary = "Publication A summary",
            };
            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Summary = "Publication B summary"
            };
            var publicationC = new Publication
            {
                Title = "Publication C",
                Slug = "publication-c",
                Summary = "Publication C summary",
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

            var releases = new List<Release>
            {
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationB,
                    Published = new DateTime(2020, 2, 1),
                },
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationC,
                    Published = new DateTime(2020, 3, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var testFile = new FileInfo()
                {
                    Name = "Test file 1",
                    Type = FileType.Data,
                };

                var releaseFileService = new Mock<IReleaseFileService>();

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[0].Id)))
                    .ReturnsAsync(ListOf(testFile));
                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[1].Id)))
                    .ReturnsAsync(ListOf(testFile));
                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[2].Id)))
                    .ReturnsAsync(ListOf(testFile));

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Equal(2, result.Count);
                Assert.Equal("Theme A", result[0].Title);
                Assert.Equal("Theme A summary", result[0].Summary);

                Assert.Equal("Theme B", result[1].Title);
                Assert.Equal("Theme B summary", result[1].Summary);

                Assert.Equal(2, result[0].Topics.Count);
                Assert.Equal("Topic A", result[0].Topics[0].Title);
                Assert.Equal("Topic B", result[0].Topics[1].Title);

                Assert.Single(result[1].Topics);
                Assert.Equal("Topic C", result[1].Topics[0].Title);

                var topicAPublications = result[0].Topics[0].Publications;

                Assert.Single(topicAPublications);
                Assert.Equal("publication-a", topicAPublications[0].Slug);
                Assert.Equal("Publication A summary", topicAPublications[0].Summary);
                Assert.Equal("Publication A", topicAPublications[0].Title);
                Assert.Equal("Academic Year 2020/21", topicAPublications[0].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", topicAPublications[0].LatestReleaseTime);
                Assert.Equal(releases[0].Id, topicAPublications[0].LatestReleaseId);

                Assert.Single(topicAPublications[0].DownloadFiles);
                Assert.Equal(testFile, topicAPublications[0].DownloadFiles[0]);

                var topicBPublications = result[0].Topics[1].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("publication-b", topicBPublications[0].Slug);
                Assert.Equal("Publication B summary", topicBPublications[0].Summary);
                Assert.Equal("Publication B", topicBPublications[0].Title);
                Assert.Equal("Academic Year 2020/21", topicBPublications[0].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", topicBPublications[0].LatestReleaseTime);
                Assert.Equal(releases[1].Id, topicBPublications[0].LatestReleaseId);

                Assert.Single(topicBPublications[0].DownloadFiles);
                Assert.Equal(testFile, topicBPublications[0].DownloadFiles[0]);

                var topicCPublications = result[1].Topics[0].Publications;

                Assert.Single(topicCPublications);
                Assert.Equal("publication-c", topicCPublications[0].Slug);
                Assert.Equal("Publication C summary", topicCPublications[0].Summary);
                Assert.Equal("Publication C", topicCPublications[0].Title);
                Assert.Equal("Academic Year 2020/21", topicCPublications[0].EarliestReleaseTime);
                Assert.Equal("Academic Year 2020/21", topicCPublications[0].LatestReleaseTime);
                Assert.Equal(releases[2].Id, topicCPublications[0].LatestReleaseId);

                Assert.Single(topicCPublications[0].DownloadFiles);
                Assert.Equal(testFile, topicCPublications[0].DownloadFiles[0]);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_FiltersThemesWithNoTopicsOrPublications()
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
                            Publications = ListOf(publicationA)
                        },
                    },
                },
                new()
                {
                    Title = "Theme C",
                }
            };

            var releases = new List<Release>
            {
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var testFile = new FileInfo()
                {
                    Name = "Test file 1",
                    Type = FileType.Data,
                };

                var releaseFileService = new Mock<IReleaseFileService>();

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[0].Id)))
                    .ReturnsAsync(ListOf(testFile));

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Single(result);
                Assert.Equal("Theme B", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_FiltersThemesWithNoVisiblePublications()
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
                            Publications = ListOf(publicationA),
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

            var releases = new List<Release>
            {
                // Published and has files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationB,
                    Published = new DateTime(2020, 1, 1),
                },
                // Not published
                new()
                {
                    ReleaseName = "2021",
                    Publication = publicationA,
                },
                // Published but does not have files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationD,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

                var testFile = new FileInfo()
                {
                    Name = "Test file 1",
                    Type = FileType.Data,
                };

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[0].Id)))
                    .ReturnsAsync(ListOf(testFile));

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[2].Id)))
                    .ReturnsAsync(new List<FileInfo>());

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Single(result);
                Assert.Equal("Theme B", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic B", result[0].Topics[0].Title);

                var topicBPublications = result[0].Topics[0].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_FiltersTopicsWithNoVisiblePublications()
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
                            Publications = ListOf(publicationB)
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

            var releases = new List<Release>
            {
                // Published and has files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationB,
                    Published = new DateTime(2020, 1, 1),
                },
                // Not published
                new()
                {
                    ReleaseName = "2021",
                    Publication = publicationA,
                },
                // Published but does not have files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationD,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(themes);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>();

                var testFile = new FileInfo()
                {
                    Name = "Test file 1",
                    Type = FileType.Data,
                };

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[0].Id)))
                    .ReturnsAsync(ListOf(testFile));

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[2].Id)))
                    .ReturnsAsync(new List<FileInfo>());

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic B", result[0].Topics[0].Title);

                var topicBPublications = result[0].Topics[0].Publications;

                Assert.Single(topicBPublications);
                Assert.Equal("Publication B", topicBPublications[0].Title);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        [Fact]
        public async Task GetPublicationDownloadsTree_FiltersPublicationsWithNoVisibleReleases()
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

            var releases = new List<Release>
            {
                // Published and has files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationA,
                    Published = new DateTime(2020, 1, 1),
                },
                // Not published
                new()
                {
                    ReleaseName = "2021",
                    Publication = publicationB,
                },
                // Published but has no files
                new()
                {
                    ReleaseName = "2020",
                    Publication = publicationD,
                    Published = new DateTime(2020, 1, 1),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(theme);
                await context.AddRangeAsync(releases);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>();

                var testFile = new FileInfo()
                {
                    Name = "Test file 1",
                    Type = FileType.Data,
                };

                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[0].Id)))
                    .ReturnsAsync(ListOf(testFile));
                releaseFileService
                    .Setup(s => s.ListDownloadFiles(
                        It.Is<Release>(r => r.Id == releases[2].Id)))
                    .ReturnsAsync(new List<FileInfo>());

                var service = BuildThemeService(context, releaseFileService: releaseFileService.Object);

                var result = await service.GetPublicationDownloadsTree();

                Assert.Single(result);
                Assert.Equal("Theme A", result[0].Title);

                Assert.Single(result[0].Topics);
                Assert.Equal("Topic A", result[0].Topics[0].Title);

                var publications = result[0].Topics[0].Publications;

                Assert.Single(publications);
                Assert.Equal("Publication A", publications[0].Title);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        private static ThemeService BuildThemeService(
            ContentDbContext contentDbContext,
            IReleaseFileService? releaseFileService = null)
        {
            return new(
                contentDbContext,
                releaseFileService ?? Mock.Of<IReleaseFileService>()
            );
        }
    }
}