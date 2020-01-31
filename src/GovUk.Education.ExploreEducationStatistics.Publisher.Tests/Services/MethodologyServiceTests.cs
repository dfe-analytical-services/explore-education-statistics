using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public void GetTree()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "GetMethodologyTree");
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

                var methodologyA = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-a",
                    Title = "Methodology A",
                    Summary = "first methodology"
                };

                var methodologyB = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-b",
                    Title = "Methodology B",
                    Summary = "second methodology"
                };

                var publicationA = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    TopicId = topic.Id,
                    Slug = "publication-a",
                    Summary = "first publication",
                    MethodologyId = methodologyA.Id
                };

                var publicationB = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    TopicId = topic.Id,
                    Slug = "publication-b",
                    Summary = "second publication",
                    MethodologyId = methodologyB.Id
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

                context.AddRange(new List<Methodology>
                {
                    methodologyA, methodologyB
                });

                context.Add(theme);
                context.Add(topic);

                context.AddRange(new List<Publication>
                {
                    publicationA, publicationB
                });

                context.AddRange(new List<Release>
                {
                    publicationARelease1, publicationBRelease1
                });

                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new MethodologyService(context, MapperForProfile<MappingProfiles>());

                var result = service.GetTree(Enumerable.Empty<Guid>());

                Assert.Single(result);
                var theme = result.First();
                Assert.Equal("Theme A", theme.Title);

                Assert.Single(theme.Topics);
                var topic = theme.Topics.First();
                Assert.Equal("Topic A", topic.Title);

                Assert.Single(topic.Publications);
                var methodology = topic.Publications.First();
                Assert.Equal("methodology-a", methodology.Slug);
                Assert.Equal("first methodology", methodology.Summary);
                Assert.Equal("Methodology A", methodology.Title);
            }
        }
    }
}