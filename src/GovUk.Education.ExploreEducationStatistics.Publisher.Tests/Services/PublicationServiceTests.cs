#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ1,
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ1,
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ1,
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ2,
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ4,
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
            TimePeriodCoverage = TimeIdentifier.AcademicYearQ1,
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
                TimePeriodCoverage = TimeIdentifier.AcademicYearQ3,
                Published = null,
                Slug = "publication-a-release-2018-q3",
                ApprovalStatus = Draft,
                Version = 0,
                PreviousVersionId = null
            },
            PublicationBRelease1
        );

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

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                Assert.True(await service.IsPublicationPublished(publication.Id));
            }
        }

        [Fact]
        public async Task IsPublicationPublished_FalseWhenPublicationHasNoReleases()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

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

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                Assert.False(await service.IsPublicationPublished(publication.Id));
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_SetsCorrectReleaseInTimeSeries()
        {
            var release2021Week21 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.Week21,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week2,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week3,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week10 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week10,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            // Latest in time series
            var release2022Week21 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week21,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    // Set the releases so that they are not in time series order

                    // Include a range of releases to make sure they are being correctly ordered by the
                    // time identifiers enum entry position rather than in alphanumeric order by value

                    release2022Week3,
                    release2022Week21,
                    release2022Week10,
                    release2021Week21,
                    release2022Week2,
                    release2022Week1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                await service.UpdateLatestPublishedRelease(publication.Id);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Release 2022 week 21 should be the latest published
                Assert.Equal(release2022Week21.Id, found.LatestPublishedReleaseId);
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_SetsCorrectReleaseInTimeSeries_LatestInSeriesNotPublished()
        {
            var release2021Week21 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.Week21,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week2,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var release2022Week3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week3,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            // Latest published in time series
            var release2022Week10 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week10,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            // Latest in time series (not published)
            var release2022Week21 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.Week21,
                Published = null
            };

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    // Set the releases so that they are not in time series order

                    // Include a range of releases to make sure they are being correctly ordered by the
                    // time identifiers enum entry position rather than in alphanumeric order by value

                    release2022Week3,
                    release2022Week21,
                    release2022Week10,
                    release2021Week21,
                    release2022Week2,
                    release2022Week1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                await service.UpdateLatestPublishedRelease(publication.Id);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Release 2022 week 10 should be the latest published
                Assert.Equal(release2022Week10.Id, found.LatestPublishedReleaseId);
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_SetsCorrectReleaseVersion()
        {
            var releaseVersion0 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 0,
                PreviousVersionId = null,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var releaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 1,
                PreviousVersionId = releaseVersion0.Id,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var releaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 2,
                PreviousVersionId = releaseVersion1.Id,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    // Set the releases so that they are not in version order
                    releaseVersion1,
                    releaseVersion2,
                    releaseVersion0
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                await service.UpdateLatestPublishedRelease(publication.Id);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Release version 2 should be the latest published
                Assert.Equal(releaseVersion2.Id, found.LatestPublishedReleaseId);
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_SetsCorrectReleaseVersion_LatestVersionNotPublished()
        {
            var releaseVersion0 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 0,
                PreviousVersionId = null,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var releaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 1,
                PreviousVersionId = releaseVersion0.Id,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            // Latest published version
            var releaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 2,
                PreviousVersionId = releaseVersion1.Id,
                Published = DateTime.UtcNow.AddDays(-1)
            };

            // Latest version (not published)
            var releaseVersion3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2022",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Version = 3,
                PreviousVersionId = releaseVersion2.Id,
                Published = null
            };

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    // Set the releases so that they are not in version order
                    releaseVersion1,
                    releaseVersion3,
                    releaseVersion2,
                    releaseVersion0
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                await service.UpdateLatestPublishedRelease(publication.Id);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Release version 2 should be the latest published
                Assert.Equal(releaseVersion2.Id, found.LatestPublishedReleaseId);
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_PublicationWithoutReleasesThrowsError()
        {
            // Set up a publication with no releases
            var publication = new Publication
            {
                Releases = new List<Release>()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.UpdateLatestPublishedRelease(publication.Id));

                Assert.Equal(
                    $"Expected publication to have at least one published release. Publication id: {publication.Id}",
                    exception.Message);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Publication shouldn't have been updated since it has no releases
                Assert.Null(found.LatestPublishedReleaseId);
            }
        }

        [Fact]
        public async Task UpdateLatestPublishedRelease_PublicationWithoutPublishedReleasesThrowsError()
        {
            // Set up a publication where the only release is not published
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        ReleaseName = "2022",
                        TimePeriodCoverage = TimeIdentifier.CalendarYear,
                        Published = null
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = SetupService(context);
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.UpdateLatestPublishedRelease(publication.Id));

                Assert.Equal(
                    $"Expected publication to have at least one published release. Publication id: {publication.Id}",
                    exception.Message);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

                // Publication shouldn't have been updated since the only release is not published
                Assert.Null(found.LatestPublishedReleaseId);
            }
        }

        private static PublicationService SetupService(
            ContentDbContext contentDbContext
        )
        {
            return new(
                contentDbContext: contentDbContext
            );
        }
    }
}
