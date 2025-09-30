using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class ReleaseSubjectServiceTests
{
    [Fact]
    public async Task Find_ReleaseId()
    {
        var releaseSubject = new ReleaseSubject
        {
            Subject = new Subject(),
            ReleaseVersion = new ReleaseVersion(),
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
                releaseVersionId: releaseSubject.ReleaseVersionId
            );

            var actualReleaseSubject = result.AssertRight();
            Assert.Equal(releaseSubject.ReleaseVersionId, actualReleaseSubject.ReleaseVersionId);
            Assert.Equal(releaseSubject.SubjectId, actualReleaseSubject.SubjectId);
        }
    }

    [Fact]
    public async Task Find_NoReleaseId()
    {
        var subject = new Subject();

        var previousReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-2),
            Version = 0,
        };

        var latestReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            Version = 1,
        };

        var futureReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1),
            Version = 2,
        };

        var releaseSubjectPreviousRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = previousReleaseVersion.Id },
        };

        var releaseSubjectLatestRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = latestReleaseVersion.Id },
        };

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = futureReleaseVersion.Id },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseSubject.AddRange(
                releaseSubjectLatestRelease,
                releaseSubjectFutureRelease,
                releaseSubjectPreviousRelease
            );
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseVersions.AddRange(
                latestReleaseVersion,
                futureReleaseVersion,
                previousReleaseVersion
            );

            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);

            var result = await service.Find(subject.Id);

            var actualReleaseSubject = result.AssertRight();
            Assert.Equal(
                releaseSubjectLatestRelease.ReleaseVersionId,
                actualReleaseSubject.ReleaseVersionId
            );
            Assert.Equal(releaseSubjectLatestRelease.SubjectId, actualReleaseSubject.SubjectId);
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion()
    {
        var subject = new Subject();

        var previousReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-2),
            Version = 0,
        };

        var latestReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            Version = 1,
        };

        var futureReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1),
            Version = 2,
        };

        var releaseSubjectPreviousRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = previousReleaseVersion.Id },
        };

        var releaseSubjectLatestRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = latestReleaseVersion.Id },
        };

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = subject,
            ReleaseVersion = new ReleaseVersion { Id = futureReleaseVersion.Id },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseSubject.AddRange(
                releaseSubjectLatestRelease,
                releaseSubjectFutureRelease,
                releaseSubjectPreviousRelease
            );
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseVersions.AddRange(
                latestReleaseVersion,
                futureReleaseVersion,
                previousReleaseVersion
            );

            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);

            var result = await service.FindForLatestPublishedVersion(subject.Id);

            Assert.NotNull(result);
            Assert.Equal(releaseSubjectLatestRelease.ReleaseVersionId, result!.ReleaseVersionId);
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
        var futureReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(1),
        };

        // Link the Subject to a Release with a future Published date/time that should not be considered Live
        var releaseSubjectFutureRelease = new ReleaseSubject
        {
            Subject = new Subject(),
            ReleaseVersion = new ReleaseVersion { Id = futureReleaseVersion.Id },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubjectFutureRelease);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseVersions.AddRange(futureReleaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(statisticsDbContext, contentDbContext);
            Assert.Null(
                await service.FindForLatestPublishedVersion(releaseSubjectFutureRelease.SubjectId)
            );
        }
    }

    private static ReleaseSubjectService BuildService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext
    )
    {
        return new ReleaseSubjectService(
            statisticsDbContext: statisticsDbContext,
            contentDbContext: contentDbContext
        );
    }
}
