using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public void GetTree()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "GetPublicationTree");
            var options = builder.Options;

            using (var context = new ContentDbContext(options))
            {
                var theme = new Theme
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme A",
                    Slug = "theme-a",
                    Summary = "The first theme"
                };

                var topic = new Topic
                {
                    Id = Guid.NewGuid(),
                    Title = "Topic A",
                    ThemeId = theme.Id,
                    Slug = "topic-a",
                    Summary = "The first topic"
                };

                var publicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    TopicId = topic.Id,
                    Slug = "publication-a",
                    Summary = "first publication"
                };

                var publicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    TopicId = topic.Id,
                    Slug = "publication-b",
                    Summary = "second publication"
                };

                var publicationC = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    TopicId = topic.Id,
                    Slug = "publication-c",
                    Summary = "third publication",
                    LegacyPublicationUrl = new Uri("http://legacy.url/")
                };

                var publicationARelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationA.Id,
                    ReleaseName = "2018",
                    TimePeriodCoverage = AcademicYearQ1,
                    Published = new DateTime(2019, 1, 01),
                    Status = Approved
                };

                var publicationBRelease1 = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publicationB.Id,
                    ReleaseName = "2018",
                    TimePeriodCoverage = AcademicYearQ1,
                    Published = null,
                    Status = Draft
                };

                context.Add(theme);
                context.Add(topic);

                context.AddRange(new List<Publication>
                {
                    publicationA, publicationB, publicationC
                });

                context.AddRange(new List<Release>
                {
                    publicationARelease1, publicationBRelease1
                });

                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var releaseService = new Mock<IReleaseService>();

                var service = new PublicationService(context, MapperForProfile<MappingProfiles>(), releaseService.Object);

                var result = service.GetTree(Enumerable.Empty<Guid>());

                Assert.Single(result);
                var theme = result.First();
                Assert.Equal("Theme A", theme.Title);

                Assert.Single(theme.Topics);
                var topic = theme.Topics.First();
                Assert.Equal("Topic A", topic.Title);

                var publications = topic.Publications;
                Assert.Equal(2, publications.Count);
                Assert.Equal("publication-a", publications[0].Slug);
                Assert.Equal("first publication", publications[0].Summary);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Null(publications[0].LegacyPublicationUrl);
                Assert.Equal("publication-c", publications[1].Slug);
                Assert.Equal("third publication", publications[1].Summary);
                Assert.Equal("Publication C", publications[1].Title);
                Assert.Equal("http://legacy.url/", publications[1].LegacyPublicationUrl);
            }
        }
    }
}