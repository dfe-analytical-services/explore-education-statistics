#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-3882 Remove after migration has been run by EES-3894
/// </summary>
public class PublicationMigrationServiceTests
{
    [Fact]
    public async Task SetLatestPublishedReleases_SetsCorrectReleaseInTimeSeries()
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

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Release 2022 week 21 should be the latest published
            await AssertLatestPublishedReleaseIdEqual(release2022Week21.Id, publication.Id, context);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_SetsCorrectReleaseInTimeSeries_LatestInSeriesNotPublished()
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

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Release 2022 week 10 should be the latest published
            await AssertLatestPublishedReleaseIdEqual(release2022Week10.Id, publication.Id, context);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_SetsCorrectReleaseVersion()
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

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Release version 2 should be the latest published 
            await AssertLatestPublishedReleaseIdEqual(releaseVersion2.Id, publication.Id, context);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_SetsCorrectReleaseVersion_LatestVersionNotPublished()
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

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Release version 2 should be the latest published
            await AssertLatestPublishedReleaseIdEqual(releaseVersion2.Id, publication.Id, context);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_IgnoresMigratedPublications()
    {
        var release = new Release
        {
            Id = Guid.NewGuid(),
            ReleaseName = "2022",
            TimePeriodCoverage = TimeIdentifier.CalendarYear,
            Published = DateTime.UtcNow.AddDays(-1)
        };

        // Set up a publication with a published release, but with a LatestPublishedReleaseId that's different to the id
        // of the release. We can use this to test if it's touched or not.
        var publication = new Publication
        {
            LatestPublishedReleaseId = Guid.NewGuid(),
            Releases = new List<Release>
            {
                release
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Releases.AddAsync(release);
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var found = await context.Publications.SingleAsync(p => p.Id == publication.Id);

            // Publication shouldn't have been updated since it already had a value of LatestPublishedReleaseId
            Assert.Equal(publication.LatestPublishedReleaseId, found.LatestPublishedReleaseId);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_IgnoresPublicationWithoutReleases()
    {
        // Published
        var publicationA = new Publication
        {
            Releases = new List<Release>
            {
                new()
                {
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        // Not published (no releases)
        var publicationB = new Publication
        {
            Releases = new List<Release>()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddRangeAsync(publicationA, publicationB);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Publication A should have a LatestPublishedReleaseId since it's published 
            await AssertLatestPublishedReleaseIdEqual(publicationA.Releases[0].Id, publicationA.Id, context);

            // Publication B should have no LatestPublishedReleaseId
            await AssertLatestPublishedReleaseIdNull(publicationB.Id, context);
        }
    }

    [Fact]
    public async Task SetLatestPublishedReleases_IgnoresPublicationWithoutPublishedReleases()
    {
        // Published
        var publicationA = new Publication
        {
            Releases = new List<Release>
            {
                new()
                {
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        // Not published (only release is not published) 
        var publicationB = new Publication
        {
            Releases = new List<Release>
            {
                new()
                {
                    ReleaseName = "2018",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = null
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddRangeAsync(publicationA, publicationB);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupService(context);
            var result = await service.SetLatestPublishedReleases();
            result.AssertRight();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            // Publication A should have a LatestPublishedReleaseId since it's published 
            await AssertLatestPublishedReleaseIdEqual(publicationA.Releases[0].Id, publicationA.Id, context);

            // Publication B should have no LatestPublishedReleaseId
            await AssertLatestPublishedReleaseIdNull(publicationB.Id, context);
        }
    }

    private static async Task AssertLatestPublishedReleaseIdEqual(Guid? expected,
        Guid publicationId,
        ContentDbContext context)
    {
        var publication = await context.Publications.SingleAsync(publication => publication.Id == publicationId);
        Assert.Equal(expected, publication.LatestPublishedReleaseId);
    }

    private static async Task AssertLatestPublishedReleaseIdNull(Guid publicationId,
        ContentDbContext context)
    {
        var publication = await context.Publications.SingleAsync(publication => publication.Id == publicationId);
        Assert.Null(publication.LatestPublishedReleaseId);
    }

    private static PublicationMigrationService SetupService(
        ContentDbContext? context = null,
        IUserService? userService = null,
        ILogger<PublicationMigrationService>? logger = null)

    {
        return new PublicationMigrationService(
            context ?? DbUtils.InMemoryApplicationDbContext(),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            logger ?? Mock.Of<ILogger<PublicationMigrationService>>()
        );
    }
}
