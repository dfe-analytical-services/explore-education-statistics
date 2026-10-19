using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class ReleaseSubjectServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Find_ReleaseId()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        ReleaseFile releaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile(FileType.Data));

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            var result = await service.Find(
                subjectId: releaseFile.File.SubjectId!.Value,
                releaseVersionId: releaseFile.ReleaseVersionId
            );

            var actualReleaseFile = result.AssertRight();
            Assert.Equal(releaseFile.ReleaseVersionId, actualReleaseFile.ReleaseVersionId);
            Assert.Equal(releaseFile.File.SubjectId, actualReleaseFile.File.SubjectId);
        }
    }

    [Fact]
    public async Task Find_NoReleaseId()
    {
        var subjectId = Guid.NewGuid();

        var previousReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(-2),
            Version = 0,
        };

        var latestReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(-1),
            Version = 1,
        };

        var futureReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(1),
            Version = 2,
        };

        File file = _fixture.DefaultFile(FileType.Data).WithSubjectId(subjectId);

        ReleaseFile releaseFilePreviousRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(previousReleaseVersion)
            .WithFile(file);

        ReleaseFile releaseFileLatestRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(latestReleaseVersion)
            .WithFile(file);

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        ReleaseFile releaseFileFutureRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(futureReleaseVersion)
            .WithFile(file);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseFiles.AddRange(
                releaseFileLatestRelease,
                releaseFileFutureRelease,
                releaseFilePreviousRelease
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            var result = await service.Find(subjectId);

            var actualReleaseFile = result.AssertRight();
            Assert.Equal(releaseFileLatestRelease.ReleaseVersionId, actualReleaseFile.ReleaseVersionId);
            Assert.Equal(subjectId, actualReleaseFile.File.SubjectId);
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion()
    {
        var subjectId = Guid.NewGuid();

        var previousReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(-2),
            Version = 0,
        };

        var latestReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(-1),
            Version = 1,
        };

        var futureReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(1),
            Version = 2,
        };

        File file = _fixture.DefaultFile(FileType.Data).WithSubjectId(subjectId);

        ReleaseFile releaseFilePreviousRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(previousReleaseVersion)
            .WithFile(file);

        ReleaseFile releaseFileLatestRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(latestReleaseVersion)
            .WithFile(file);

        // Link the Subject to the next version of the Release with a future Published date/time
        // that should not be considered Live
        ReleaseFile releaseFileFutureRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(futureReleaseVersion)
            .WithFile(file);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseFiles.AddRange(
                releaseFileLatestRelease,
                releaseFileFutureRelease,
                releaseFilePreviousRelease
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            var result = await service.FindForLatestPublishedVersion(subjectId);

            Assert.NotNull(result);
            Assert.Equal(releaseFileLatestRelease.ReleaseVersionId, result!.ReleaseVersionId);
            Assert.Equal(subjectId, result.File.SubjectId);
        }
    }

    [Fact]
    public async Task FindForLatestPublishedVersion_NoReleases()
    {
        await using var contentDbContext = InMemoryContentDbContext();

        var service = BuildService(contentDbContext);
        Assert.Null(await service.FindForLatestPublishedVersion(Guid.NewGuid()));
    }

    [Fact]
    public async Task FindForLatestPublishedVersion_NoPublishedReleases()
    {
        var futureReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTimeOffset.UtcNow.AddDays(1),
        };

        // Link the Subject to a Release with a future Published date/time that should not be considered Live
        ReleaseFile releaseFileFutureRelease = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(futureReleaseVersion)
            .WithFile(_fixture.DefaultFile(FileType.Data));

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFileFutureRelease);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);
            Assert.Null(await service.FindForLatestPublishedVersion(releaseFileFutureRelease.File.SubjectId!.Value));
        }
    }

    private static ReleaseSubjectService BuildService(ContentDbContext contentDbContext)
    {
        return new ReleaseSubjectService(contentDbContext: contentDbContext);
    }
}
