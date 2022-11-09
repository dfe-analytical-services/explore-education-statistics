#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class PublicationRepositoryTests
{
    [Fact]
    public async Task IsPublicationPublished_FalseWhenPublicationHasNoPublishedRelease()
    {
        var publication = new Publication
        {
            LatestPublishedReleaseId = null
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildPublicationRepository(contentDbContext);

            Assert.False(await service.IsPublished(publication.Id));
        }
    }

    [Fact]
    public async Task IsPublicationPublished_TrueWhenPublicationHasPublishedRelease()
    {
        var publication = new Publication
        {
            LatestPublishedReleaseId = Guid.NewGuid()
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildPublicationRepository(contentDbContext);

            Assert.True(await service.IsPublished(publication.Id));
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            await service.UpdateLatestPublishedRelease(publication.Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            await service.UpdateLatestPublishedRelease(publication.Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            await service.UpdateLatestPublishedRelease(publication.Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            await service.UpdateLatestPublishedRelease(publication.Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLatestPublishedRelease(publication.Id));

            Assert.Equal(
                $"Expected publication to have at least one published release. Publication id: {publication.Id}",
                exception.Message);
        }

        await using (var context = InMemoryContentDbContext(contextId))
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

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildPublicationRepository(context);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLatestPublishedRelease(publication.Id));

            Assert.Equal(
                $"Expected publication to have at least one published release. Publication id: {publication.Id}",
                exception.Message);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

            // Publication shouldn't have been updated since the only release is not published
            Assert.Null(found.LatestPublishedReleaseId);
        }
    }

    private static PublicationRepository BuildPublicationRepository(
        ContentDbContext contentDbContext)
    {
        return new(
            contentDbContext: contentDbContext
        );
    }
}
