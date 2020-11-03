using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class TaxonomyServiceTests
    {
        [Fact]
        public async Task SyncStatisticsTaxonomy_AddsNewTheme()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Slug = "theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Topic 1",
                        Slug = "topic-1",
                    },
                    new Topic
                    {
                        Title = "Topic 2",
                        Slug = "topic-2",
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(t => t.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Theme 1", themes[0].Title);
                Assert.Equal("theme-1", themes[0].Slug);

                var topics = themes[0].Topics.ToList();
                Assert.Equal(2, topics.Count);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Topic 1", topics[0].Title);
                Assert.Equal("topic-1", topics[0].Slug);

                Assert.Equal(theme1.Topics[1].Id, topics[1].Id);
                Assert.Equal("Topic 2", topics[1].Title);
                Assert.Equal("topic-2", topics[1].Slug);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_RemovesTheme()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Slug = "theme-1",
                Topics = new List<Topic>()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Title = "Theme 2",
                    Slug = "theme-2",
                    Topics = new HashSet<Data.Model.Topic>()
                });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(t => t.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Theme 1", themes[0].Title);
                Assert.Equal("theme-1", themes[0].Slug);

                Assert.Empty(themes[0].Topics);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_UpdatesTheme()
        {
            var theme1 = new Theme
            {
                Title = "Updated Theme 1",
                Slug = "updated-theme-1",
                Topics = new List<Topic>()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Old Theme 1",
                    Slug = "old-theme-1",
                    Topics = new HashSet<Data.Model.Topic>()
                });

                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(t => t.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Updated Theme 1", themes[0].Title);
                Assert.Equal("updated-theme-1", themes[0].Slug);

                Assert.Empty(themes[0].Topics);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_UpdatesThemeAndTopic()
        {
            var theme1 = new Theme
            {
                Title = "Updated Theme 1",
                Slug = "updated-theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Updated Topic 1",
                        Slug = "updated-topic-1",
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Old Theme 1",
                    Slug = "old-theme-1",
                    Topics = new HashSet<Data.Model.Topic>
                    {
                        new Data.Model.Topic
                        {
                            Id = theme1.Topics[0].Id,
                            Title = "Old Topic 1",
                            Slug = "old-topic-1"
                        }
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Updated Theme 1", themes[0].Title);
                Assert.Equal("updated-theme-1", themes[0].Slug);

                var topics = themes[0].Topics.ToList();
                Assert.Single(topics);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Updated Topic 1", topics[0].Title);
                Assert.Equal("updated-topic-1", topics[0].Slug);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_UpdatesThemeAndAddsNewTopic()
        {
            var theme1 = new Theme
            {
                Title = "Updated Theme 1",
                Slug = "updated-theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Updated Topic 1",
                        Slug = "updated-topic-1",
                    },
                    new Topic
                    {
                        Title = "Topic 2",
                        Slug = "topic-2",
                    },
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Old Theme 1",
                    Slug = "old-theme-1",
                    Topics = new HashSet<Data.Model.Topic>
                    {
                        new Data.Model.Topic
                        {
                            Id = theme1.Topics[0].Id,
                            Title = "Old Topic 1",
                            Slug = "old-topic-1"
                        },
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Updated Theme 1", themes[0].Title);
                Assert.Equal("updated-theme-1", themes[0].Slug);

                var topics = themes[0].Topics.ToList();
                Assert.Equal(2, topics.Count);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Updated Topic 1", topics[0].Title);
                Assert.Equal("updated-topic-1", topics[0].Slug);

                Assert.Equal(theme1.Topics[1].Id, topics[1].Id);
                Assert.Equal("Topic 2", topics[1].Title);
                Assert.Equal("topic-2", topics[1].Slug);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_UpdatesThemeAndRemovesTopic()
        {
            var theme1 = new Theme
            {
                Title = "Updated Theme 1",
                Slug = "updated-theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Topic 1",
                        Slug = "topic-1",
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Old Theme 1",
                    Slug = "old-theme-1",
                    Topics = new HashSet<Data.Model.Topic>
                    {
                        new Data.Model.Topic
                        {
                            Id = theme1.Topics[0].Id,
                            Title = "Topic 1",
                            Slug = "topic-1"
                        },
                        new Data.Model.Topic
                        {
                            Title = "Topic 2",
                            Slug = "topic-2"
                        },
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ToList();

                Assert.Single(themes);

                Assert.Equal(theme1.Id, themes[0].Id);
                Assert.Equal("Updated Theme 1", themes[0].Title);
                Assert.Equal("updated-theme-1", themes[0].Slug);

                var topics = themes[0].Topics.ToList();
                Assert.Single(topics);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Topic 1", topics[0].Title);
                Assert.Equal("topic-1", topics[0].Slug);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_DoesNotAddTopicPublication()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Slug = "theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Updated Topic 1",
                        Slug = "updated-topic-1",
                        Publications = new List<Publication>
                        {
                            new Publication
                            {
                                Title = "Publication 1"
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Theme 1",
                    Slug = "theme-1",
                    Topics = new HashSet<Data.Model.Topic>
                    {
                        new Data.Model.Topic
                        {
                            Id = theme1.Topics[0].Id,
                            Title = "Old Topic 1",
                            Slug = "old-topic-1",
                            Publications = new List<Data.Model.Publication>()
                        }
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ThenInclude(topic => topic.Publications)
                    .ToList();

                Assert.Single(themes);

                var topics = themes[0].Topics.ToList();
                Assert.Single(topics);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Updated Topic 1", topics[0].Title);
                Assert.Equal("updated-topic-1", topics[0].Slug);

                Assert.Empty(topics[0].Publications);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_DoesNotUpdateTopicPublication()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Slug = "theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Updated Topic 1",
                        Slug = "updated-topic-1",
                        Publications = new List<Publication>
                        {
                            new Publication
                            {
                                Title = "Updated publication 1"
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = theme1.Id,
                    Title = "Theme 1",
                    Slug = "theme-1",
                    Topics = new HashSet<Data.Model.Topic>
                    {
                        new Data.Model.Topic
                        {
                            Id = theme1.Topics[0].Id,
                            Title = "Old Topic 1",
                            Slug = "old-topic-1",
                            Publications = new List<Data.Model.Publication>
                            {
                                new Data.Model.Publication
                                {
                                    Title = "Publication 1"
                                }
                            }
                        }
                    }
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ThenInclude(topic => topic.Publications)
                    .ToList();

                Assert.Single(themes);

                var topics = themes[0].Topics.ToList();
                Assert.Single(topics);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Updated Topic 1", topics[0].Title);
                Assert.Equal("updated-topic-1", topics[0].Slug);

                Assert.Single(topics[0].Publications);
                Assert.Equal("Publication 1", topics[0].Publications.First().Title);
            }
        }

        [Fact]
        public async Task SyncStatisticsTaxonomy_DoesNotRemoveTopicPublication()
        {
            var theme1 = new Theme
            {
                Title = "Theme 1",
                Slug = "theme-1",
                Topics = new List<Topic>
                {
                    new Topic
                    {
                        Title = "Updated Topic 1",
                        Slug = "updated-topic-1",
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.AddAsync(theme1);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.AddAsync(
                    new Data.Model.Theme
                    {
                        Id = theme1.Id,
                        Title = "Theme 1",
                        Slug = "theme-1",
                        Topics = new HashSet<Data.Model.Topic>
                        {
                            new Data.Model.Topic
                            {
                                Id = theme1.Topics[0].Id,
                                Title = "Old Topic 1",
                                Slug = "old-topic-1",
                                Publications = new List<Data.Model.Publication>
                                {
                                    new Data.Model.Publication
                                    {
                                        Title = "Publication 1"
                                    }
                                }
                            }
                        }
                    }
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTaxonomyService(contentDbContext, statisticsDbContext);

                await service.SyncTaxonomy();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var themes = statisticsDbContext.Theme
                    .Include(theme => theme.Topics)
                    .ThenInclude(topic => topic.Publications)
                    .ToList();

                Assert.Single(themes);

                var topics = themes[0].Topics.ToList();
                Assert.Single(topics);

                Assert.Equal(theme1.Topics[0].Id, topics[0].Id);
                Assert.Equal("Updated Topic 1", topics[0].Title);
                Assert.Equal("updated-topic-1", topics[0].Slug);

                Assert.Single(topics[0].Publications);
                Assert.Equal("Publication 1", topics[0].Publications.First().Title);
            }
        }

        private TaxonomyService BuildTaxonomyService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext)
        {
            return new TaxonomyService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                mapper: MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}