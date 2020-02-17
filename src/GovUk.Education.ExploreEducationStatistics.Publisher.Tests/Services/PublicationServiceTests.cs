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
        private static readonly Theme Theme = new Theme
        {
            Id = Guid.NewGuid(),
            Title = "Theme A",
            Slug = "theme-a",
            Summary = "The first theme"
        };

        private static readonly Topic Topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "Topic A",
            ThemeId = Theme.Id,
            Slug = "topic-a",
            Summary = "The first topic"
        };

        private static readonly Contact Contact1 = new Contact
        {
            Id = Guid.NewGuid(),
            ContactName = "first contact name",
            ContactTelNo = "first contact tel no",
            TeamEmail = "first@contact.com",
            TeamName = "first contact team name"
        };

        private static readonly Contact Contact2 = new Contact
        {
            Id = Guid.NewGuid(),
            ContactName = "second contact name",
            ContactTelNo = "second contact tel no",
            TeamEmail = "second@contact.com",
            TeamName = "second contact team name"
        };

        private static readonly Contact Contact3 = new Contact
        {
            Id = Guid.NewGuid(),
            ContactName = "third contact name",
            ContactTelNo = "third contact tel no",
            TeamEmail = "third@contact.com",
            TeamName = "third contact team name"
        };

        private static readonly Methodology Methodology = new Methodology
        {
            Id = Guid.NewGuid(),
            Title = "methodology title",
            Slug = "methodology-slug",
            Summary = "methodology summary",
            Published = new DateTime(2020, 2, 10),
            LastUpdated = new DateTime(2020, 2, 11)
        };

        private static readonly Publication PublicationA = new Publication
        {
            Id = Guid.NewGuid(),
            Contact = Contact1,
            Title = "Publication A",
            TopicId = Topic.Id,
            DataSource = "first publication data source",
            Description = "first publication description",
            ExternalMethodology = new ExternalMethodology
            {
              Title  = "external methodology title",
              Url = new Uri("http://external.methodology/")
            },
            LegacyReleases = new List<Link>
            {
              new Link
              {
                  Id = Guid.NewGuid(),
                  Description = "2008 to 2009",
                  Url = "http://link.one/"
              },
              new Link
              {
                  Id = Guid.NewGuid(),
                  Description = "2010 to 2011",
                  Url = "http://link.three/"
              },
              new Link
              {
                  Id = Guid.NewGuid(),
                  Description = "2009 to 2010",
                  Url = "http://link.two/"
              }
            },
            MethodologyId = Methodology.Id,
            Slug = "publication-a",
            Summary = "first publication summary",
        };

        private static readonly Publication PublicationB = new Publication
        {
            Id = Guid.NewGuid(),
            Contact = Contact2,
            Title = "Publication B",
            TopicId = Topic.Id,
            DataSource = "second publication data source",
            Description = "second publication description",
            Slug = "publication-b",
            Summary = "second publication summary"
        };

        private static readonly Publication PublicationC = new Publication
        {
            Id = Guid.NewGuid(),
            Contact = Contact3,
            Title = "Publication C",
            TopicId = Topic.Id,
            DataSource = "third publication data source",
            Description = "third publication description",
            Slug = "publication-c",
            Summary = "third publication summary",
            LegacyPublicationUrl = new Uri("http://legacy.url/")
        };

        private static readonly List<Publication> Publications = new List<Publication>
        {
            PublicationA, PublicationB, PublicationC
        };
        
        private static readonly Release PublicationARelease1 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q1",
            Status = Approved
        };

        private static readonly Release PublicationARelease2 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q2",
            Status = Approved
        };

        private static readonly Release PublicationARelease3 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2017",
            TimePeriodCoverage = AcademicYearQ4,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2017-q4",
            Status = Approved
        };

        private static readonly Release PublicationBRelease1 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationB.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = null,
            Slug = "publication-b-release-2018-q1",
            Status = Draft
        };

        private static readonly List<Release> Releases = new List<Release>
        {
            PublicationARelease1,
            PublicationARelease2,
            PublicationARelease3,
            new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = PublicationA.Id,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ3,
                Published = null,
                Slug = "publication-a-release-2018-q3",
                Status = Draft
            },
            PublicationBRelease1
        };
        
        [Fact]
        public void GetTree()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "GetPublicationTree");
            var options = builder.Options;

            using (var context = new ContentDbContext(options))
            {
                context.Add(Theme);
                context.Add(Topic);
                context.AddRange(Publications);
                context.AddRange(Releases);

                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var releaseService = new Mock<IReleaseService>();

                var service =
                    new PublicationService(context, MapperForProfile<MappingProfiles>(), releaseService.Object);

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
                Assert.Equal("first publication summary", publications[0].Summary);
                Assert.Equal("Publication A", publications[0].Title);
                Assert.Null(publications[0].LegacyPublicationUrl);
                Assert.Equal("publication-c", publications[1].Slug);
                Assert.Equal("third publication summary", publications[1].Summary);
                Assert.Equal("Publication C", publications[1].Title);
                Assert.Equal("http://legacy.url/", publications[1].LegacyPublicationUrl);
            }
        }

        [Fact]
        public void GetViewModelAsync()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "GetPublicationViewModel");
            var options = builder.Options;

            using (var context = new ContentDbContext(options))
            {
                context.Add(Methodology);
                context.Add(Theme);
                context.Add(Topic);
                context.AddRange(Publications);
                context.AddRange(Releases);

                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var releaseService = new Mock<IReleaseService>();

                releaseService.Setup(s => s.GetLatestRelease(PublicationA.Id, Enumerable.Empty<Guid>()))
                    .Returns(PublicationARelease1);

                var service =
                    new PublicationService(context, MapperForProfile<MappingProfiles>(), releaseService.Object);

                var result = service.GetViewModelAsync(PublicationA.Id, Enumerable.Empty<Guid>());
                Assert.True(result.IsCompleted);
                var viewModel = result.Result;

                Assert.Equal(PublicationA.Id, viewModel.Id);
                Assert.Equal("Publication A", viewModel.Title);
                Assert.Equal("publication-a", viewModel.Slug);
                Assert.Equal("first publication description", viewModel.Description);
                Assert.Equal("first publication data source", viewModel.DataSource);
                Assert.Equal("first publication summary", viewModel.Summary);
                Assert.Equal(PublicationARelease1.Id, viewModel.LatestReleaseId);

                Assert.NotNull(viewModel.Topic);
                var topic = viewModel.Topic;

                Assert.NotNull(topic.Theme);
                var theme = topic.Theme;
                Assert.Equal(Theme.Title, theme.Title);

                Assert.NotNull(viewModel.Contact);
                var contact = viewModel.Contact;
                Assert.Equal("first contact name", contact.ContactName);
                Assert.Equal("first contact tel no", contact.ContactTelNo);
                Assert.Equal("first@contact.com", contact.TeamEmail);
                Assert.Equal("first contact team name", contact.TeamName);
                
                Assert.NotNull(viewModel.ExternalMethodology);
                var externalMethodology = viewModel.ExternalMethodology;
                Assert.Equal("external methodology title", externalMethodology.Title);
                Assert.Equal("http://external.methodology/", externalMethodology.Url);
                
                Assert.NotNull(viewModel.LegacyReleases);
                var legacyReleases = viewModel.LegacyReleases;
                Assert.Equal(3, legacyReleases.Count);
                Assert.Equal("2008 to 2009", legacyReleases[0].Description);
                Assert.Equal("http://link.one/", legacyReleases[0].Url);
                Assert.Equal("2009 to 2010", legacyReleases[1].Description);
                Assert.Equal("http://link.two/", legacyReleases[1].Url);
                Assert.Equal("2010 to 2011", legacyReleases[2].Description);
                Assert.Equal("http://link.three/", legacyReleases[2].Url);
                
                Assert.NotNull(viewModel.Methodology);
                var methodology = viewModel.Methodology;
                Assert.Equal(Methodology.Id, methodology.Id);
                Assert.Equal("methodology-slug", methodology.Slug);
                Assert.Equal("methodology summary", methodology.Summary);
                Assert.Equal("methodology title", methodology.Title);
                
                Assert.NotNull(viewModel.Releases);
                var releases = viewModel.Releases;
                Assert.Equal(3, releases.Count);
                Assert.Equal(PublicationARelease3.Id, releases[0].Id);
                Assert.Equal("publication-a-release-2017-q4", releases[0].Slug);
                Assert.Equal("Academic Year Q4 2017/18", releases[0].Title);
                Assert.Equal(PublicationARelease1.Id, releases[1].Id);
                Assert.Equal("publication-a-release-2018-q1", releases[1].Slug);
                Assert.Equal("Academic Year Q1 2018/19", releases[1].Title);
                Assert.Equal(PublicationARelease2.Id, releases[2].Id);
                Assert.Equal("publication-a-release-2018-q2", releases[2].Slug);
                Assert.Equal("Academic Year Q2 2018/19", releases[2].Title);
            }
        }
    }
}