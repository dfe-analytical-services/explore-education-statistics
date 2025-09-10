using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class ReleaseFileRepositoryTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task
        CheckLinkedOriginalAndReplacementReleaseFilesExist_Success()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Data))
            .Generate();

        var replacementReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Data)
                .WithReplacing(originalReleaseFile.File))
            .Generate();

        originalReleaseFile.File.ReplacedById = replacementReleaseFile.FileId;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFileRepository = new ReleaseFileRepository(contentDbContext);
            var either = await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                releaseVersion.Id, originalReleaseFile.FileId);

            var releaseFiles = either.AssertRight();

            var originalReleaseFileResult = releaseFiles.originalReleaseFile;
            var replacementReleaseFileResult = releaseFiles.replacementReleaseFile;

            Assert.Equal(releaseVersion.Id, originalReleaseFileResult.ReleaseVersionId);
            Assert.Equal(originalReleaseFile.FileId, originalReleaseFileResult.FileId);

            Assert.Equal(releaseVersion.Id, replacementReleaseFileResult.ReleaseVersionId);
            Assert.Equal(replacementReleaseFile.FileId, replacementReleaseFileResult.FileId);
        }
    }

    [Fact]
    public async Task
        CheckLinkedOriginalAndReplacementReleaseFilesExist_OriginalFileDoesNotExist()
    {
        var releaseVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);
        var releaseFileRepository = new ReleaseFileRepository(contentDbContext);
        var either = await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersionId, Guid.NewGuid());

        either.AssertNotFound();
    }

    [Fact]
    public async Task
        CheckLinkedOriginalAndReplacementReleaseFilesExist_ReplacementFileDoesNotExist()
    {
        var releaseVersionId = Guid.NewGuid();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersionId)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Data))
            .Generate();

        originalReleaseFile.File.ReplacedById = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(originalReleaseFile);
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFileRepository = new ReleaseFileRepository(contentDbContext);
            var either = await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                releaseVersionId, originalReleaseFile.FileId);

            either.AssertNotFound();
        }
    }

    [Fact]
    public async Task
        CheckLinkedOriginalAndReplacementReleaseFilesExist_ReplacementFileWrongType()
    {
        var releaseVersionId = Guid.NewGuid();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersionId)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Data))
            .Generate();

        var replacementReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersionId)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Ancillary)
                .WithReplacing(originalReleaseFile.File))
            .Generate();

        originalReleaseFile.File.ReplacedById = replacementReleaseFile.FileId;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFileRepository = new ReleaseFileRepository(contentDbContext);
            var either = await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                releaseVersionId, originalReleaseFile.FileId);

            either.AssertNotFound();
        }
    }

    [Fact]
    public async Task
        CheckLinkedOriginalAndReplacementReleaseFilesExist_ReplacementFileNullReplacingId()
    {
        var releaseVersionId = Guid.NewGuid();

        var originalReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersionId)
            .WithFile(_fixture
                .DefaultFile()
                .WithType(FileType.Data))
            .Generate();

        var replacementReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersionId(releaseVersionId)
            .WithFile(_fixture
                .DefaultFile()
                // No Replacing set
                .WithType(FileType.Data))
            .Generate();

        originalReleaseFile.File.ReplacedById = replacementReleaseFile.FileId;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFileRepository = new ReleaseFileRepository(contentDbContext);
            var either = await releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                releaseVersionId, originalReleaseFile.FileId);

            either.AssertNotFound();
        }
    }
}
