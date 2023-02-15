using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using ContentRelease = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class ReleaseSubjectServiceTests
{
    [Fact]
    public async Task Find_ReleaseId()
    {
        var releaseSubject = new ReleaseSubject
        {
            Subject = new Subject(),
            Release = new Release()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);

            var result = await service.Find(
                subjectId: releaseSubject.SubjectId,
                releaseId: releaseSubject.ReleaseId);

            var actualReleaseSubject = result.AssertRight();
            Assert.Equal(releaseSubject.ReleaseId, actualReleaseSubject.ReleaseId);
            Assert.Equal(releaseSubject.SubjectId, actualReleaseSubject.SubjectId);
        }
    }

    [Fact]
    public async Task Find_NoReleaseId()
    {
        var subject = new Subject();

        var previousReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-2),
            Version = 0
        };

        var latestReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            Version = 1
        };

        var futureReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1),
            Version = 2
        };

        var releaseSubjectPreviousRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = previousReleaseVersion.Id,
            }
        };

        var releaseSubjectLatestRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = latestReleaseVersion.Id
            }
        };

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = futureReleaseVersion.Id
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(
                releaseSubjectLatestRelease,
                releaseSubjectFutureRelease,
                releaseSubjectPreviousRelease);
            await statisticsDbContext.SaveChangesAsync();

            await contentDbContext.Releases.AddRangeAsync(
                latestReleaseVersion,
                futureReleaseVersion,
                previousReleaseVersion);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);

            var result = await service.Find(subject.Id);

            var actualReleaseSubject = result.AssertRight();
            Assert.Equal(releaseSubjectLatestRelease.ReleaseId, actualReleaseSubject.ReleaseId);
            Assert.Equal(releaseSubjectLatestRelease.SubjectId, actualReleaseSubject.SubjectId);
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion()
    {
        var subject = new Subject();

        var previousReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-2),
            Version = 0
        };

        var latestReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            Version = 1
        };

        var futureReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1),
            Version = 2
        };

        var releaseSubjectPreviousRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = previousReleaseVersion.Id,
            }
        };

        var releaseSubjectLatestRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = latestReleaseVersion.Id
            }
        };

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = subject,
            Release = new Release
            {
                Id = futureReleaseVersion.Id
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(
                releaseSubjectLatestRelease,
                releaseSubjectFutureRelease,
                releaseSubjectPreviousRelease);
            await statisticsDbContext.SaveChangesAsync();

            await contentDbContext.Releases.AddRangeAsync(
                latestReleaseVersion,
                futureReleaseVersion,
                previousReleaseVersion);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);

            var result = await service.FindForLatestPublishedVersion(subject.Id);

            Assert.NotNull(result);
            Assert.Equal(releaseSubjectLatestRelease.ReleaseId, result!.ReleaseId);
            Assert.Equal(releaseSubjectLatestRelease.SubjectId, result.SubjectId);
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion_NoReleases()
    {
        var subject = new Subject();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);
            Assert.Null(await service.FindForLatestPublishedVersion(subject.Id));
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion_NoPublishedReleases()
    {
        var futureReleaseVersion = new ContentRelease
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1)
        };

        // Link the Subject to a Release with a future Published date/time that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = new Subject(),
            Release = new Release
            {
                Id = futureReleaseVersion.Id
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubjectFutureRelease);
            await statisticsDbContext.SaveChangesAsync();

            await contentDbContext.Releases.AddRangeAsync(futureReleaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);
            Assert.Null(
                await service.FindForLatestPublishedVersion(releaseSubjectFutureRelease.SubjectId));
        }
    }

    private static ReleaseSubjectService BuildService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext)
    {
        return new ReleaseSubjectService(
            statisticsDbContext: statisticsDbContext,
            contentDbContext: contentDbContext
        );
    }
}
