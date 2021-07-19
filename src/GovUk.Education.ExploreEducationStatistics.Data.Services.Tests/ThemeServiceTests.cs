using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ThemeServiceTests
    {
        [Fact]
        public async Task ListThemesWithLiveSubjects()
        {
            var releaseFile1 = new ReleaseFile
            {
                Release = new Release
                {
                    ReleaseName = "2000",
                    Published = DateTime.UtcNow,
                    Publication = new Publication
                    {
                        Title = "Theme 1 topic 1 publication 1",
                        Slug = "theme-a-topic-a-publication-1",
                        Topic = new Topic
                        {
                            Title = "Theme 1 topic 1",
                            Slug = "theme-a-topic-a",
                            Theme = new Theme
                            {
                                Title = "Theme 1",
                                Slug = "theme-a",
                            }
                        }
                    }
                },
                File = new File
                {
                    Type = FileType.Data
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = new Release
                {
                    ReleaseName = "2000",
                    Published = DateTime.UtcNow,
                    Publication = new Publication
                    {
                        Title = "Theme B topic B publication B",
                        Slug = "theme-b-topic-b-publication-B",
                        Topic = new Topic
                        {
                            Title = "Theme B topic B",
                            Slug = "theme-b-topic-b",
                            Theme = new Theme
                            {
                                Title = "Theme B",
                                Slug = "theme-b",
                            }
                        }
                    }
                },
                File = new File
                {
                    Type = FileType.Data
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                await context.AddRangeAsync(
                    releaseFile1,
                    releaseFile2
                );
                await context.SaveChangesAsync();
            }

            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.ListThemesWithLiveSubjects().ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Theme.Id, result[0].Id);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Theme.Slug, result[0].Slug);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Theme.Title, result[0].Title);

                var themeATopics = result[0].Topics.ToList();
                Assert.Single(themeATopics);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Id, themeATopics[0].Id);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Slug, themeATopics[0].Slug);
                Assert.Equal(releaseFile1.Release.Publication.Topic.Title, themeATopics[0].Title);

                var themeATopicAPublications = themeATopics[0].Publications.ToList();
                Assert.Single(themeATopicAPublications);
                Assert.Equal(releaseFile1.Release.Publication.Id, themeATopicAPublications[0].Id);
                Assert.Equal(releaseFile1.Release.Publication.Slug, themeATopicAPublications[0].Slug);
                Assert.Equal(releaseFile1.Release.Publication.Title, themeATopicAPublications[0].Title);

                Assert.Equal(releaseFile2.Release.Publication.Topic.Theme.Id, result[1].Id);
                Assert.Equal(releaseFile2.Release.Publication.Topic.Theme.Slug, result[1].Slug);
                Assert.Equal(releaseFile2.Release.Publication.Topic.Theme.Title, result[1].Title);

                var themeBTopics = result[1].Topics.ToList();
                Assert.Single(themeBTopics);
                Assert.Equal(releaseFile2.Release.Publication.Topic.Id, themeBTopics[0].Id);
                Assert.Equal(releaseFile2.Release.Publication.Topic.Slug, themeBTopics[0].Slug);
                Assert.Equal(releaseFile2.Release.Publication.Topic.Title, themeBTopics[0].Title);

                var themeBTopicAPublications = themeBTopics[0].Publications.ToList();
                Assert.Single(themeBTopicAPublications);
                Assert.Equal(releaseFile2.Release.Publication.Id, themeBTopicAPublications[0].Id);
                Assert.Equal(releaseFile2.Release.Publication.Slug, themeBTopicAPublications[0].Slug);
                Assert.Equal(releaseFile2.Release.Publication.Title, themeBTopicAPublications[0].Title);
            }
        }

        [Fact]
        public void ListThemesWithLiveSubject_ThemeHasNoTopics()
        {
            using (var context = InMemoryContentDbContext())
            {
                var theme = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme",
                    Slug = "theme"
                };

                context.Add(theme);

                context.SaveChanges();

                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.ListThemesWithLiveSubjects());
            }
        }

        [Fact]
        public void ListThemesWithLiveSubject_TopicHasNoPublications()
        {
            using (var context = InMemoryContentDbContext())
            {
                var theme = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme",
                    Slug = "theme"
                };

                var topic = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Topic",
                    Slug = "topic",
                    ThemeId = theme.Id
                };

                context.Add(theme);
                context.Add(topic);

                context.SaveChanges();

                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.ListThemesWithLiveSubjects());
            }
        }

        [Fact]
        public void ListThemesWithLiveSubject_PublicationHasNoReleases()
        {
            using (var context = InMemoryContentDbContext())
            {
                var theme = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme",
                    Slug = "theme"
                };

                var topic = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Topic",
                    Slug = "topic",
                    ThemeId = theme.Id
                };

                var publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication",
                    Slug = "publication",
                    TopicId = topic.Id
                };

                context.Add(theme);
                context.Add(topic);
                context.Add(publication);

                context.SaveChanges();

                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.ListThemesWithLiveSubjects());
            }
        }

        [Fact]
        public void ListThemesWithLiveSubject_PublicationHasReleasesNotPublished()
        {
            var theme = new Theme
            {
                Title = "Theme",
                Slug = "theme"
            };

            var topic = new Topic
            {
                Title = "Topic",
                Slug = "topic",
                Theme = theme
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                Slug = "publication-a",
                Topic = topic
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                Slug = "publication-b",
                Topic = topic
            };

            var publicationARelease = new Release
            {
                Publication = publicationA,
                ReleaseName = "2000",
                Published = DateTime.UtcNow
            };

            var publicationBRelease = new Release
            {
                Publication = publicationB,
                ReleaseName = "2000",
                Published = null
            };

            var publicationAReleaseFile = new ReleaseFile
            {
                Release = publicationARelease,
                File = new File
                {
                    Type = FileType.Data
                }
            };

            var publicationBReleaseFile = new ReleaseFile
            {
                Release = publicationBRelease,
                File = new File
                {
                    Type = FileType.Data
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.AddRange(publicationAReleaseFile, publicationBReleaseFile);
                context.SaveChanges();
            }

            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.ListThemesWithLiveSubjects().ToList();

                Assert.Single(result);
                Assert.Equal(theme.Id, result[0].Id);
                Assert.Equal(theme.Slug, result[0].Slug);
                Assert.Equal(theme.Title, result[0].Title);
                var topics = result[0].Topics.ToList();

                Assert.Single(topics);
                Assert.Equal(topic.Id, topics[0].Id);
                Assert.Equal(topic.Slug, topics[0].Slug);
                Assert.Equal(topic.Title, topics[0].Title);
                var publications = topics[0].Publications.ToList();

                Assert.Single(publications);
                Assert.Equal(publicationA.Id, publications[0].Id);
                Assert.Equal(publicationA.Slug, publications[0].Slug);
                Assert.Equal(publicationA.Title, publications[0].Title);

                // Publication B is not included because it's Release is not published
            }
        }

        [Fact]
        public void ListThemesWithLiveSubject_PublicationLatestReleaseHasNoSubject()
        {
            var theme = new Theme();
            var topic = new Topic { Theme = theme };
            var publication = new Publication { Topic = topic };
            var previousRelease = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                Published = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1))
            };
            var previousReleaseFile = new ReleaseFile
            {
                Release = previousRelease,
                File = new File { Type = FileType.Data }
            };

            var latestRelease = new Release
            {
                Publication = publication,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
                PreviousVersion = previousRelease
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                context.AddRange(previousReleaseFile, latestRelease);
                context.SaveChanges();
            }

            using (var context = InMemoryContentDbContext(contentDbContextId))
            {
                var service =
                    new ThemeService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.ListThemesWithLiveSubjects().ToList();

                Assert.Empty(result);
            }
        }
    }
}
