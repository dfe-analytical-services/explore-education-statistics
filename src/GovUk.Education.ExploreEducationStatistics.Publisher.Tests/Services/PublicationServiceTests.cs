using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class PublicationServiceTests
    {
        private static readonly Theme Theme = new()
        {
            Id = Guid.NewGuid(),
            Title = "Theme A",
            Slug = "theme-a",
            Summary = "The first theme"
        };

        private static readonly Topic Topic = new()
        {
            Id = Guid.NewGuid(),
            Title = "Topic A",
            ThemeId = Theme.Id,
            Slug = "topic-a"
        };

        private static readonly Contact Contact1 = new()
        {
            Id = Guid.NewGuid(),
            ContactName = "first contact name",
            ContactTelNo = "first contact tel no",
            TeamEmail = "first@contact.com",
            TeamName = "first contact team name"
        };

        private static readonly Contact Contact2 = new()
        {
            Id = Guid.NewGuid(),
            ContactName = "second contact name",
            ContactTelNo = "second contact tel no",
            TeamEmail = "second@contact.com",
            TeamName = "second contact team name"
        };

        private static readonly Contact Contact3 = new()
        {
            Id = Guid.NewGuid(),
            ContactName = "third contact name",
            ContactTelNo = "third contact tel no",
            TeamEmail = "third@contact.com",
            TeamName = "third contact team name"
        };

        private static readonly Publication PublicationA = new()
        {
            Id = Guid.NewGuid(),
            Contact = Contact1,
            Title = "Publication A",
            TopicId = Topic.Id,
            ExternalMethodology = new ExternalMethodology
            {
                Title = "external methodology title",
                Url = "http://external.methodology/"
            },
            LegacyReleases = AsList(
                new LegacyRelease
                {
                    Id = Guid.NewGuid(),
                    Description = "Academic Year 2008/09",
                    Order = 0,
                    Url = "http://link.one/"
                },
                new LegacyRelease
                {
                    Id = Guid.NewGuid(),
                    Description = "Academic Year 2010/11",
                    Order = 2,
                    Url = "http://link.three/"
                },
                new LegacyRelease
                {
                    Id = Guid.NewGuid(),
                    Description = "Academic Year 2009/10",
                    Order = 1,
                    Url = "http://link.two/"
                }
            ),
            Slug = "publication-a",
            LegacyPublicationUrl = new Uri("http://legacy.url/")
        };

        private static readonly Publication PublicationB = new()
        {
            Id = Guid.NewGuid(),
            Contact = Contact2,
            Title = "Publication B",
            TopicId = Topic.Id,
            Slug = "publication-b",
        };

        private static readonly Publication PublicationC = new()
        {
            Id = Guid.NewGuid(),
            Contact = Contact3,
            Title = "Publication C",
            TopicId = Topic.Id,
            Slug = "publication-c",
            LegacyPublicationUrl = new Uri("http://legacy.url/")
        };

        private static readonly Release PublicationARelease1V0 = new()
        {
            Id = new Guid("240ca03c-6c22-4b9d-9f15-40fc9017890e"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q1",
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly Release PublicationARelease1V1Deleted = new()
        {
            Id = new Guid("cf02f125-91da-4606-bf80-c2058092a653"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q1",
            ApprovalStatus = Approved,
            Version = 1,
            PreviousVersionId = new Guid("240ca03c-6c22-4b9d-9f15-40fc9017890e"),
            SoftDeleted = true
        };

        private static readonly Release PublicationARelease1V1 = new()
        {
            Id = new Guid("9da67d6d-a75f-424d-8b8b-975f151292a4"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q1",
            ApprovalStatus = Approved,
            Version = 1,
            PreviousVersionId = new Guid("240ca03c-6c22-4b9d-9f15-40fc9017890e")
        };

        private static readonly Release PublicationARelease2 = new()
        {
            Id = new Guid("874d4e4f-5568-482f-a5a4-d41e5bf6632a"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2018-q2",
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly Release PublicationARelease3 = new()
        {
            Id = new Guid("676ff979-9b1d-4bd2-a3f1-f126c4e2e8d4"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2017",
            TimePeriodCoverage = AcademicYearQ4,
            Published = new DateTime(2019, 1, 01),
            Slug = "publication-a-release-2017-q4",
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly Release PublicationBRelease1 = new()
        {
            Id = new Guid("e66247d7-b350-4d81-a223-3080edc55623"),
            PublicationId = PublicationB.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = null,
            Slug = "publication-b-release-2018-q1",
            ApprovalStatus = Draft,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly List<Release> Releases = AsList(
            PublicationARelease1V0,
            PublicationARelease1V1Deleted,
            PublicationARelease1V1,
            PublicationARelease2,
            PublicationARelease3,
            new Release
            {
                Id = new Guid("3c7b1338-4c41-43b4-b4ae-67c21c8734fb"),
                PublicationId = PublicationA.Id,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ3,
                Published = null,
                Slug = "publication-a-release-2018-q3",
                ApprovalStatus = Draft,
                Version = 0,
                PreviousVersionId = null
            },
            PublicationBRelease1
        );

        [Fact]
        public async Task GetViewModel()
        {
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(Theme);
                await context.AddAsync(Topic);
                await context.AddRangeAsync(PublicationA, PublicationB, PublicationC);
                await context.AddRangeAsync(Releases);
                await context.SaveChangesAsync();
            }

            using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseService = new Mock<IReleaseService>();

                releaseService.Setup(s => s.GetLatestRelease(PublicationA.Id, Enumerable.Empty<Guid>()))
                    .ReturnsAsync(PublicationARelease1V1);

                var service = BuildPublicationService(context, releaseService: releaseService.Object);

                var result = await service.GetViewModel(PublicationA.Id, Enumerable.Empty<Guid>());

                Assert.Equal(PublicationA.Id, result.Id);
                Assert.Equal("Publication A", result.Title);
                Assert.Equal("publication-a", result.Slug);
                Assert.Equal(PublicationARelease1V1.Id, result.LatestReleaseId);
                Assert.Contains(PublicationARelease1V1.Id, result.Releases.Select(r => r.Id));
                Assert.DoesNotContain(PublicationARelease1V0.Id, result.Releases.Select(r => r.Id));
                Assert.DoesNotContain(PublicationARelease1V1Deleted.Id, result.Releases.Select(r => r.Id));

                Assert.NotNull(result.Topic);
                var topic = result.Topic;

                Assert.NotNull(topic.Theme);
                var theme = topic.Theme;
                Assert.Equal(Theme.Title, theme.Title);

                Assert.NotNull(result.Contact);
                var contact = result.Contact;
                Assert.Equal("first contact name", contact.ContactName);
                Assert.Equal("first contact tel no", contact.ContactTelNo);
                Assert.Equal("first@contact.com", contact.TeamEmail);
                Assert.Equal("first contact team name", contact.TeamName);

                Assert.NotNull(result.ExternalMethodology);
                var externalMethodology = result.ExternalMethodology;
                Assert.Equal("external methodology title", externalMethodology.Title);
                Assert.Equal("http://external.methodology/", externalMethodology.Url);

                Assert.NotNull(result.LegacyReleases);
                var legacyReleases = result.LegacyReleases;
                Assert.Equal(3, legacyReleases.Count);
                Assert.Equal("Academic Year 2010/11", legacyReleases[0].Description);
                Assert.Equal("http://link.three/", legacyReleases[0].Url);
                Assert.Equal("Academic Year 2009/10", legacyReleases[1].Description);
                Assert.Equal("http://link.two/", legacyReleases[1].Url);
                Assert.Equal("Academic Year 2008/09", legacyReleases[2].Description);
                Assert.Equal("http://link.one/", legacyReleases[2].Url);

                Assert.NotNull(result.Releases);
                var releases = result.Releases;
                Assert.Equal(3, releases.Count);
                Assert.Equal(PublicationARelease2.Id, releases[0].Id);
                Assert.Equal("publication-a-release-2018-q2", releases[0].Slug);
                Assert.Equal("Academic Year Q2 2018/19", releases[0].Title);
                Assert.Equal(PublicationARelease1V1.Id, releases[1].Id);
                Assert.Equal("publication-a-release-2018-q1", releases[1].Slug);
                Assert.Equal("Academic Year Q1 2018/19", releases[1].Title);
                Assert.Equal(PublicationARelease3.Id, releases[2].Id);
                Assert.Equal("publication-a-release-2017-q4", releases[2].Slug);
                Assert.Equal("Academic Year Q4 2017/18", releases[2].Title);
            }
        }

        [Fact]
        public async Task IsPublicationPublished_TrueWhenPublicationHasPublishedReleases()
        {
            var previousRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Published = DateTime.UtcNow,
                Version = 1
            };

            var latestPublishedRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousRelease.Id,
                Published = DateTime.UtcNow,
                Version = 1
            };

            var latestDraftRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedRelease.Id,
                Published = null,
                Version = 2
            };

            var publication = new Publication
            {
                Releases = AsList(previousRelease, latestPublishedRelease, latestDraftRelease)
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildPublicationService(contentDbContext);

                Assert.True(await service.IsPublicationPublished(publication.Id));
            }
        }

        [Fact]
        public async Task IsPublicationPublished_FalseWhenPublicationHasNoReleases()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildPublicationService(contentDbContext);

                Assert.False(await service.IsPublicationPublished(publication.Id));
            }
        }

        [Fact]
        public async Task IsPublicationPublished_FalseWhenPublicationHasNoPublishedReleases()
        {
            var publication = new Publication
            {
                Releases = AsList(
                    new Release
                {
                    Published = null
                })
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildPublicationService(contentDbContext);

                Assert.False(await service.IsPublicationPublished(publication.Id));
            }
        }

        [Fact]
        public async Task SetPublishedDate_AddsStatsPublication()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic"
                }
            };

            var publishDate = DateTime.Now;

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = BuildPublicationService(contentDbContext);
                await service.SetPublishedDate(publication.Id, publishDate);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var contentPublication = await contentDbContext.Publications.FindAsync(publication.Id);

                Assert.NotNull(contentPublication);
                Assert.Equal(publication.Id, contentPublication.Id);
                Assert.Equal("Test publication", contentPublication.Title);
                Assert.Equal("test-publication", contentPublication.Slug);
                Assert.Equal(publishDate, contentPublication.Published);
            }
        }

        [Fact]
        public async Task SetPublishedDate_UpdatesStatsPublication()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic"
                }
            };

            var publishDate = DateTime.Now;

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = BuildPublicationService(contentDbContext);

                await service.SetPublishedDate(publication.Id, publishDate);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var contentPublication = await contentDbContext.Publications.FindAsync(publication.Id);

                Assert.NotNull(contentPublication);
                Assert.Equal(publication.Id, contentPublication.Id);
                Assert.Equal("Test publication", contentPublication.Title);
                Assert.Equal("test-publication", contentPublication.Slug);
                Assert.Equal(publishDate, contentPublication.Published);
            }
        }

        private PublicationService BuildPublicationService(
            ContentDbContext contentDbContext,
            IMapper mapper = null,
            IReleaseService releaseService = null
        ) {
            return new(
                contentDbContext: contentDbContext,
                mapper: mapper ?? MapperForProfile<MappingProfiles>(),
                releaseService: releaseService ?? new Mock<IReleaseService>().Object
            );
        }
    }
}
