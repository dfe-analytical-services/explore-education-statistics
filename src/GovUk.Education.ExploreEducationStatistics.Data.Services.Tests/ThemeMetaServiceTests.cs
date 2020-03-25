using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ThemeMetaServiceTests
    {
        [Fact]
        public void GetThemes()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var themeA = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A",
                    Slug = "theme-a",
                };

                var themeB = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B",
                    Slug = "theme-b",
                };

                var themeATopicA = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic A",
                    ThemeId = themeA.Id,
                    Slug = "theme-a-topic-a"
                };

                var themeATopicB = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic B",
                    ThemeId = themeA.Id,
                    Slug = "theme-a-topic-b"
                };

                var themeBTopicA = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic A",
                    ThemeId = themeB.Id,
                    Slug = "theme-b-topic-a"
                };

                var themeBTopicB = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic B",
                    ThemeId = themeB.Id,
                    Slug = "theme-b-topic-b"
                };

                var themeATopicAPublicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic A publication A",
                    TopicId = themeATopicA.Id,
                    Slug = "theme-a-topic-a-publication-a"
                };

                var themeATopicAPublicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic A publication B",
                    TopicId = themeATopicA.Id,
                    Slug = "theme-a-topic-a-publication-B"
                };

                var themeATopicBPublicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic B publication A",
                    TopicId = themeATopicB.Id,
                    Slug = "theme-a-topic-b-publication-a"
                };

                var themeATopicBPublicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A topic B publication B",
                    TopicId = themeATopicB.Id,
                    Slug = "theme-a-topic-b-publication-B"
                };

                var themeBTopicAPublicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic A publication A",
                    TopicId = themeBTopicA.Id,
                    Slug = "theme-b-topic-a-publication-a"
                };

                var themeBTopicAPublicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic A publication B",
                    TopicId = themeBTopicA.Id,
                    Slug = "theme-b-topic-a-publication-B"
                };

                var themeBTopicBPublicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic B publication A",
                    TopicId = themeBTopicB.Id,
                    Slug = "theme-b-topic-b-publication-a"
                };

                var themeBTopicBPublicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme B topic B publication B",
                    TopicId = themeBTopicB.Id,
                    Slug = "theme-b-topic-b-publication-B"
                };

                var themeATopicAPublicationARelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeATopicAPublicationA,
                    Published = DateTime.UtcNow
                };

                var themeATopicAPublicationBRelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeATopicAPublicationB,
                    Published = DateTime.UtcNow
                };

                var themeATopicBPublicationARelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeATopicBPublicationA,
                    Published = DateTime.UtcNow
                };

                var themeATopicBPublicationBRelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeATopicBPublicationB,
                    Published = DateTime.UtcNow
                };

                var themeBTopicAPublicationARelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeBTopicAPublicationA,
                    Published = DateTime.UtcNow
                };

                var themeBTopicAPublicationBRelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeBTopicAPublicationB,
                    Published = DateTime.UtcNow
                };

                var themeBTopicBPublicationARelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeBTopicBPublicationA,
                    Published = DateTime.UtcNow
                };

                var themeBTopicBPublicationBRelease = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = themeBTopicBPublicationB,
                    Published = DateTime.UtcNow
                };

                context.AddRange(new List<Theme>
                {
                    themeA, themeB
                });

                context.AddRange(new List<Topic>
                {
                    themeATopicA, themeATopicB, themeBTopicA, themeBTopicB
                });

                context.AddRange(new List<Publication>
                {
                    themeATopicAPublicationA,
                    themeATopicAPublicationB,
                    themeATopicBPublicationA,
                    themeATopicBPublicationB,
                    themeBTopicAPublicationA,
                    themeBTopicAPublicationB,
                    themeBTopicBPublicationA,
                    themeBTopicBPublicationB
                });

                context.AddRange(new List<Release>
                {
                    themeATopicAPublicationARelease,
                    themeATopicAPublicationBRelease,
                    themeATopicBPublicationARelease,
                    themeATopicBPublicationBRelease,
                    themeBTopicAPublicationARelease,
                    themeBTopicAPublicationBRelease,
                    themeBTopicBPublicationARelease,
                    themeBTopicBPublicationBRelease
                });

                context.SaveChanges();

                var service =
                    new ThemeMetaService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.GetThemes().ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal(themeA.Id, result[0].Id);
                Assert.Equal(themeA.Slug, result[0].Slug);
                Assert.Equal(themeA.Title, result[0].Title);
                var themeATopics = result[0].Topics.ToList();

                Assert.Equal(2, themeATopics.Count);
                Assert.Equal(themeATopicA.Id, themeATopics[0].Id);
                Assert.Equal(themeATopicA.Slug, themeATopics[0].Slug);
                Assert.Equal(themeATopicA.Title, themeATopics[0].Title);
                var themeATopicAPublications = themeATopics[0].Publications.ToList();

                Assert.Equal(2, themeATopicAPublications.Count);
                Assert.Equal(themeATopicAPublicationA.Id, themeATopicAPublications[0].Id);
                Assert.Equal(themeATopicAPublicationA.Slug, themeATopicAPublications[0].Slug);
                Assert.Equal(themeATopicAPublicationA.Title, themeATopicAPublications[0].Title);
                Assert.Equal(themeATopicAPublicationB.Id, themeATopicAPublications[1].Id);
                Assert.Equal(themeATopicAPublicationB.Slug, themeATopicAPublications[1].Slug);
                Assert.Equal(themeATopicAPublicationB.Title, themeATopicAPublications[1].Title);

                Assert.Equal(themeATopicB.Id, themeATopics[1].Id);
                Assert.Equal(themeATopicB.Slug, themeATopics[1].Slug);
                Assert.Equal(themeATopicB.Title, themeATopics[1].Title);
                var themeATopicBPublications = themeATopics[1].Publications.ToList();

                Assert.Equal(2, themeATopicBPublications.Count);
                Assert.Equal(themeATopicBPublicationA.Id, themeATopicBPublications[0].Id);
                Assert.Equal(themeATopicBPublicationA.Slug, themeATopicBPublications[0].Slug);
                Assert.Equal(themeATopicBPublicationA.Title, themeATopicBPublications[0].Title);
                Assert.Equal(themeATopicBPublicationB.Id, themeATopicBPublications[1].Id);
                Assert.Equal(themeATopicBPublicationB.Slug, themeATopicBPublications[1].Slug);
                Assert.Equal(themeATopicBPublicationB.Title, themeATopicBPublications[1].Title);

                Assert.Equal(themeB.Id, result[1].Id);
                Assert.Equal(themeB.Slug, result[1].Slug);
                Assert.Equal(themeB.Title, result[1].Title);
                var themeBTopics = result[1].Topics.ToList();

                Assert.Equal(2, themeBTopics.Count);
                Assert.Equal(themeBTopicA.Id, themeBTopics[0].Id);
                Assert.Equal(themeBTopicA.Slug, themeBTopics[0].Slug);
                Assert.Equal(themeBTopicA.Title, themeBTopics[0].Title);
                var themeBTopicAPublications = themeBTopics[0].Publications.ToList();

                Assert.Equal(2, themeBTopicAPublications.Count);
                Assert.Equal(themeBTopicAPublicationA.Id, themeBTopicAPublications[0].Id);
                Assert.Equal(themeBTopicAPublicationA.Slug, themeBTopicAPublications[0].Slug);
                Assert.Equal(themeBTopicAPublicationA.Title, themeBTopicAPublications[0].Title);
                Assert.Equal(themeBTopicAPublicationB.Id, themeBTopicAPublications[1].Id);
                Assert.Equal(themeBTopicAPublicationB.Slug, themeBTopicAPublications[1].Slug);
                Assert.Equal(themeBTopicAPublicationB.Title, themeBTopicAPublications[1].Title);

                Assert.Equal(themeBTopicB.Id, themeBTopics[1].Id);
                Assert.Equal(themeBTopicB.Slug, themeBTopics[1].Slug);
                Assert.Equal(themeBTopicB.Title, themeBTopics[1].Title);
                var themeBTopicBPublications = themeBTopics[1].Publications.ToList();

                Assert.Equal(2, themeBTopicBPublications.Count);
                Assert.Equal(themeBTopicBPublicationA.Id, themeBTopicBPublications[0].Id);
                Assert.Equal(themeBTopicBPublicationA.Slug, themeBTopicBPublications[0].Slug);
                Assert.Equal(themeBTopicBPublicationA.Title, themeBTopicBPublications[0].Title);
                Assert.Equal(themeBTopicBPublicationB.Id, themeBTopicBPublications[1].Id);
                Assert.Equal(themeBTopicBPublicationB.Slug, themeBTopicBPublications[1].Slug);
                Assert.Equal(themeBTopicBPublicationB.Title, themeBTopicBPublications[1].Title);
            }
        }

        [Fact]
        public void GetThemes_ThemeHasNoTopics()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
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
                    new ThemeMetaService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.GetThemes());
            }
        }

        [Fact]
        public void GetThemes_TopicHasNoPublications()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
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
                    new ThemeMetaService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.GetThemes());
            }
        }

        [Fact]
        public void GetThemes_PublicationHasNoReleases()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
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
                    new ThemeMetaService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());
                Assert.Empty(service.GetThemes());
            }
        }

        [Fact]
        public void GetThemes_PublicationHasReleasesNotPublished()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
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

                var publicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    Slug = "publication-a",
                    TopicId = topic.Id
                };

                var publicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    Slug = "publication-b",
                    TopicId = topic.Id
                };

                var publicationARelease = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationA.Id,
                    Published = DateTime.UtcNow
                };

                var publicationBRelease = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationB.Id,
                    Published = null
                };

                context.Add(theme);
                context.Add(topic);

                context.AddRange(new List<Publication>
                {
                    publicationA, publicationB
                });

                context.AddRange(new List<Release>
                {
                    publicationARelease, publicationBRelease
                });

                context.SaveChanges();

                var service =
                    new ThemeMetaService(context, MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.GetThemes().ToList();

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
    }
}