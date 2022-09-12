#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task List()
        {
            var release1 = new Release
            {
                Slug = "release-1",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
                DataLastPublished = new DateTime(2020, 1, 1),
                NextReleaseDate = new PartialDate { Year = "2020" },
                Type = ReleaseType.NationalStatistics
            };
            var release2 = new Release
            {
                Slug = "release-2",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2021, 1, 1),
                DataLastPublished = new DateTime(2021, 1, 1),
                NextReleaseDate = new PartialDate { Year = "2021" },
                Type = ReleaseType.NationalStatistics
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(release1, release2)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                Assert.Equal(2, releases.Count);

                // Ordered from most newest to oldest
                Assert.Equal(release2.Id, releases[0].Id);
                Assert.Equal(release2.Title, releases[0].Title);
                Assert.Equal(release2.ReleaseName, releases[0].ReleaseName);
                Assert.Equal(release2.Slug, releases[0].Slug);
                Assert.Equal(release2.TimePeriodCoverage.GetEnumLabel(), releases[0].CoverageTitle);
                Assert.Equal(release2.YearTitle, releases[0].YearTitle);
                Assert.Equal(release2.Published, releases[0].Published);
                release2.NextReleaseDate.AssertDeepEqualTo(releases[0].NextReleaseDate);
                Assert.Equal(release2.Type, releases[0].Type);
                Assert.Equal(release2.DataLastPublished, releases[0].DataLastPublished);
                Assert.True(releases[0].LatestRelease);

                Assert.Equal(release1.Id, releases[1].Id);
                Assert.Equal(release1.Title, releases[1].Title);
                Assert.Equal(release1.ReleaseName, releases[1].ReleaseName);
                Assert.Equal(release1.Slug, releases[1].Slug);
                Assert.Equal(release1.TimePeriodCoverage.GetEnumLabel(), releases[1].CoverageTitle);
                Assert.Equal(release1.YearTitle, releases[1].YearTitle);
                Assert.Equal(release1.Published, releases[1].Published);
                release1.NextReleaseDate.AssertDeepEqualTo(releases[1].NextReleaseDate);
                Assert.Equal(release1.Type, releases[1].Type);
                Assert.Equal(release1.DataLastPublished, releases[1].DataLastPublished);
                Assert.False(releases[1].LatestRelease);
            }
        }

        [Fact]
        public async Task List_FiltersPreviousReleasesForAmendments()
        {
            var originalRelease = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Published = new DateTime(2020, 1, 1),
                Type = ReleaseType.NationalStatistics,
            };
            var amendedRelease = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2020, 2, 1),
                Type = ReleaseType.NationalStatistics,
                PreviousVersion = originalRelease
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(originalRelease, amendedRelease)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                var releaseViewModel = Assert.Single(releases);
                Assert.Equal(amendedRelease.Id, releaseViewModel.Id);
            }
        }

        [Fact]
        public async Task List_FiltersDraftReleases()
        {
            // Draft
            var release1 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2020",
                Type = ReleaseType.NationalStatistics
            };
            // Published
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Published = new DateTime(2021, 1, 1),
                Type = ReleaseType.NationalStatistics
            };
            // Amendment is draft
            var release2Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                ReleaseName = "2021",
                Type = ReleaseType.NationalStatistics,
                PreviousVersion = release2
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Releases = ListOf(release1, release2, release2Amendment)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var service = SetupReleaseService(contentDbContext);

                var result = await service.List(publication.Slug);

                var releases = result.AssertRight();

                var releaseViewModel = Assert.Single(releases);
                Assert.Equal(release2.Id, releaseViewModel.Id);
            }
        }

        [Fact]
        public async Task List_PublicationNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List("random-slug");

            result.AssertNotFound();
        }

        private static ReleaseService SetupReleaseService(
            ContentDbContext? contentDbContext = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext is null
                    ? Mock.Of<IPersistenceHelper<ContentDbContext>>()
                    : new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? AlwaysTrueUserService().Object
            );
        }
    }
}
