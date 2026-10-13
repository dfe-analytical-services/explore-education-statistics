using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Semver;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Database;

public class DataSetFileDualWriteInterceptorTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task AddDataFileThenMetaFile_SeparateSaves()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            await AssertMirrored(contentDbContext, dataReleaseFile, metaReleaseFile.File);
        }
    }

    [Fact]
    public async Task AddDataFileAndMetaFile_SameSave()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            await AssertMirrored(contentDbContext, dataReleaseFile, metaReleaseFile.File);
        }
    }

    [Fact]
    public async Task AddMetaFileThenDataFile_SeparateSaves()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            dataReleaseFile.ReleaseVersion = null!;
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            await AssertMirrored(contentDbContext, dataReleaseFile, metaReleaseFile.File);
        }
    }

    [Fact]
    public async Task AddDataFile_Synchronous()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            // ReSharper disable once MethodHasAsyncOverload
            contentDbContext.SaveChanges();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            await AssertMirrored(contentDbContext, dataReleaseFile, metaReleaseFile.File);
        }
    }

    [Fact]
    public async Task AddDataFile_NoDataSetFileId_NotMirrored()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();
        var dataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile().WithType(FileType.Data))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Single(contentDbContext.ReleaseFiles);
            Assert.Empty(contentDbContext.DataSetFiles);
            Assert.Empty(contentDbContext.DataSetFileVersions);
            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
        }
    }

    [Fact]
    public async Task AddOtherFileTypes_NotMirrored()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();
        var ancillaryReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile(FileType.Ancillary))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(ancillaryReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Empty(contentDbContext.DataSetFiles);
            Assert.Empty(contentDbContext.DataSetFileVersions);
            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
        }
    }

    [Fact]
    public async Task AddReleaseFileByFileId_Amendment()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();
        var amendment = _fixture.DefaultReleaseVersion().Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendment);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var amendmentDataReleaseFile = new ReleaseFile
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = amendment.Id,
            FileId = dataReleaseFile.FileId,
            Name = dataReleaseFile.Name,
            Summary = dataReleaseFile.Summary,
            Order = dataReleaseFile.Order,
            PublicApiDataSetId = Guid.NewGuid(),
            PublicApiDataSetVersion = SemVersion.Parse("1.0.0", SemVersionStyles.Strict),
        };

        var amendmentMetaReleaseFile = new ReleaseFile
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = amendment.Id,
            FileId = metaReleaseFile.FileId,
        };

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.AddRange(amendmentDataReleaseFile, amendmentMetaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var links = await contentDbContext
                .ReleaseVersionDataSetFileVersions.OrderBy(link => link.Id == amendmentDataReleaseFile.Id)
                .ToListAsync();

            Assert.Equal(2, links.Count);
            Assert.Equal(dataReleaseFile.Id, links[0].Id);
            AssertLinkMirrors(amendmentDataReleaseFile, links[1]);

            Assert.Single(contentDbContext.DataSetFileVersions);
        }
    }

    [Fact]
    public async Task AddReleaseFile_DataFileNotMirrored_SaveSucceeds()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();
        var dataFile = _fixture.DefaultFile().WithType(FileType.Data).Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Files.Add(dataFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(
                new ReleaseFile
                {
                    Id = Guid.NewGuid(),
                    ReleaseVersionId = releaseVersion.Id,
                    FileId = dataFile.Id,
                }
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Single(contentDbContext.ReleaseFiles);
            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
        }
    }

    [Fact]
    public async Task AddReplacement()
    {
        var (releaseVersion, originalReleaseFile, _) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(originalReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var replacementReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(
                _fixture
                    .DefaultFile(FileType.Data)
                    .WithDataSetFileId(originalReleaseFile.File.DataSetFileId!.Value)
                    .WithReplacingId(originalReleaseFile.FileId)
            )
            .Generate();
        replacementReleaseFile.File.DataSetFileVersion = 1;

        // As per ReleaseDataFileRepository.Create
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var originalFile = await contentDbContext.Files.SingleAsync(f => f.Id == originalReleaseFile.FileId);

            replacementReleaseFile.ReleaseVersion = null!;
            await contentDbContext.ReleaseFiles.AddAsync(replacementReleaseFile);
            contentDbContext.Update(originalFile);
            originalFile.ReplacedBy = replacementReleaseFile.File;

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var dataSetFile = await contentDbContext.DataSetFiles.Include(dsf => dsf.Versions).SingleAsync();

            Assert.Equal(originalReleaseFile.File.DataSetFileId, dataSetFile.Id);
            Assert.Equal(2, dataSetFile.Versions.Count);

            var original = dataSetFile.Versions.Single(v => v.Id == originalReleaseFile.FileId);
            var replacement = dataSetFile.Versions.Single(v => v.Id == replacementReleaseFile.FileId);

            Assert.Equal(0, original.Version);
            Assert.Equal(replacement.Id, original.ReplacedById);
            Assert.Null(original.ReplacingId);

            Assert.Equal(1, replacement.Version);
            Assert.Equal(original.Id, replacement.ReplacingId);
            Assert.Null(replacement.ReplacedById);

            Assert.Equal(2, await contentDbContext.ReleaseVersionDataSetFileVersions.CountAsync());
        }
    }

    [Fact]
    public async Task ModifyDataFileAndReleaseFile()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterHierarchies = new List<DataSetFileFilterHierarchy>
        {
            new([Guid.NewGuid(), Guid.NewGuid()], [new Dictionary<Guid, List<Guid>> { { Guid.NewGuid(), [] } }]),
        };

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(rf => rf.Id == dataReleaseFile.Id);

            releaseFile.Name = "Updated name";
            releaseFile.Published = DateTime.UtcNow;
            releaseFile.FilterSequence = [new FilterSequenceEntry(Guid.NewGuid(), [])];
            releaseFile.File.FilterHierarchies = filterHierarchies;
            releaseFile.File.DataSetFileMeta = WithNumDataFileRows(releaseFile.File.DataSetFileMeta!, 1234);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(rf => rf.Id == dataReleaseFile.Id);

            Assert.Equal(1234, releaseFile.File.DataSetFileMeta!.NumDataFileRows);

            await AssertMirrored(contentDbContext, releaseFile, metaReleaseFile.File);
        }
    }

    [Fact]
    public async Task ModifyDataFile_OnlyModifiedPropertiesCopied()
    {
        var (releaseVersion, dataReleaseFile, _) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using var staleContentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId);
        var staleFile = await staleContentDbContext.Files.SingleAsync(f => f.Id == dataReleaseFile.FileId);

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var file = await contentDbContext.Files.SingleAsync(f => f.Id == dataReleaseFile.FileId);
            file.DataSetFileMeta = WithNumDataFileRows(file.DataSetFileMeta!, 1234);
            await contentDbContext.SaveChangesAsync();
        }

        staleFile.Filename = "updated.csv";
        await staleContentDbContext.SaveChangesAsync();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var version = await contentDbContext.DataSetFileVersions.SingleAsync();

            Assert.Equal("updated.csv", version.Filename);
            Assert.Equal(1234, version.DataSetFileMeta!.NumDataFileRows);
        }
    }

    [Fact]
    public async Task UpdateDataFileAndReleaseFile_NoChanges_MirrorsUnchanged()
    {
        var (releaseVersion, dataReleaseFile, _) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(rf => rf.Id == dataReleaseFile.Id);

            // Marks every property of the ReleaseFile and File as modified
            contentDbContext.Update(releaseFile);
            contentDbContext.Update(releaseFile.File);

            var savedCount = await contentDbContext.SaveChangesAsync();

            Assert.Equal(2, savedCount);
        }
    }

    [Fact]
    public async Task ModifyDataFile_NoVersion_SaveSucceeds()
    {
        var (releaseVersion, dataReleaseFile, _) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(dataReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        // Simulate drift, as deleting the mirror rows directly isn't mirrored
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersionDataSetFileVersions.RemoveRange(
                contentDbContext.ReleaseVersionDataSetFileVersions
            );
            contentDbContext.DataSetFileVersions.RemoveRange(contentDbContext.DataSetFileVersions);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(rf => rf.Id == dataReleaseFile.Id);

            releaseFile.Name = "Updated name";
            releaseFile.File.Filename = "updated.csv";

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext.ReleaseFiles.Include(rf => rf.File).SingleAsync();
            Assert.Equal("Updated name", releaseFile.Name);
            Assert.Equal("updated.csv", releaseFile.File.Filename);

            Assert.Empty(contentDbContext.DataSetFileVersions);
            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
        }
    }

    [Fact]
    public async Task DeleteDataSet()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        // As per ReleaseDataFileService.Delete, which deletes each of these in a separate save
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Remove(
                await contentDbContext.ReleaseFiles.SingleAsync(rf => rf.Id == dataReleaseFile.Id)
            );
            await contentDbContext.SaveChangesAsync();

            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
            Assert.Single(contentDbContext.DataSetFileVersions);

            contentDbContext.ReleaseFiles.Remove(
                await contentDbContext.ReleaseFiles.SingleAsync(rf => rf.Id == metaReleaseFile.Id)
            );
            await contentDbContext.SaveChangesAsync();

            contentDbContext.Files.Remove(
                await contentDbContext.Files.SingleAsync(f => f.Id == dataReleaseFile.FileId)
            );
            await contentDbContext.SaveChangesAsync();

            Assert.Empty(contentDbContext.DataSetFileVersions);
            Assert.Empty(contentDbContext.DataSetFiles);

            contentDbContext.Files.Remove(
                await contentDbContext.Files.SingleAsync(f => f.Id == metaReleaseFile.FileId)
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Empty(contentDbContext.Files);
            Assert.Empty(contentDbContext.DataSetFiles);
            Assert.Empty(contentDbContext.DataSetFileVersions);
            Assert.Empty(contentDbContext.ReleaseVersionDataSetFileVersions);
        }
    }

    [Fact]
    public async Task DeleteDataFile_OtherVersionsRemain_DataSetFileKept()
    {
        var (releaseVersion, originalReleaseFile, _) = GenerateDataSet();

        var otherVersionReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(
                _fixture.DefaultFile(FileType.Data).WithDataSetFileId(originalReleaseFile.File.DataSetFileId!.Value)
            )
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, otherVersionReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Remove(
                await contentDbContext.ReleaseFiles.SingleAsync(rf => rf.Id == originalReleaseFile.Id)
            );
            contentDbContext.Files.Remove(
                await contentDbContext.Files.SingleAsync(f => f.Id == originalReleaseFile.FileId)
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var dataSetFile = await contentDbContext.DataSetFiles.Include(dsf => dsf.Versions).SingleAsync();
            var version = Assert.Single(dataSetFile.Versions);
            Assert.Equal(otherVersionReleaseFile.FileId, version.Id);
        }
    }

    [Fact]
    public async Task DeleteMetaFile_MetaColumnsCleared()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Remove(
                await contentDbContext.ReleaseFiles.SingleAsync(rf => rf.Id == metaReleaseFile.Id)
            );
            contentDbContext.Files.Remove(
                await contentDbContext.Files.SingleAsync(f => f.Id == metaReleaseFile.FileId)
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var version = await contentDbContext.DataSetFileVersions.SingleAsync();

            Assert.Equal(dataReleaseFile.FileId, version.Id);
            Assert.Null(version.MetaFileId);
            Assert.Null(version.MetaFilename);
            Assert.Null(version.MetaContentType);
            Assert.Null(version.MetaContentLength);
        }
    }

    [Fact]
    public async Task SavingChangesCalledAgain_NoDuplicates()
    {
        var (releaseVersion, dataReleaseFile, metaReleaseFile) = GenerateDataSet();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(dataReleaseFile, metaReleaseFile);

            // As if a previous SaveChanges had failed after the interceptor had run
            await DataSetFileDualWriteInterceptor.Instance.SavingChangesAsync(
                new DbContextEventData(null!, null!, contentDbContext),
                default
            );

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            await AssertMirrored(contentDbContext, dataReleaseFile, metaReleaseFile.File);
        }
    }

    private (ReleaseVersion, ReleaseFile dataReleaseFile, ReleaseFile metaReleaseFile) GenerateDataSet()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataFile = _fixture.DefaultFile(FileType.Data).Generate();
        var metaFile = _fixture
            .DefaultFile(FileType.Metadata)
            .WithSubjectId(dataFile.SubjectId!.Value)
            .WithRootPath(dataFile.RootPath)
            .Generate();

        var dataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(dataFile)
            .WithFilterSequence([new FilterSequenceEntry(Guid.NewGuid(), [])])
            .WithIndicatorSequence([new IndicatorGroupSequenceEntry(Guid.NewGuid(), [Guid.NewGuid()])])
            .Generate();

        var metaReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(metaFile)
            .Generate();

        return (releaseVersion, dataReleaseFile, metaReleaseFile);
    }

    private static DataSetFileMeta WithNumDataFileRows(DataSetFileMeta meta, int numDataFileRows)
    {
        var copy = JsonConvert.DeserializeObject<DataSetFileMeta>(JsonConvert.SerializeObject(meta))!;
        copy.NumDataFileRows = numDataFileRows;
        return copy;
    }

    private static async Task AssertMirrored(
        ContentDbContext contentDbContext,
        ReleaseFile dataReleaseFile,
        File metaFile
    )
    {
        var dataFile = dataReleaseFile.File;

        var dataSetFile = await contentDbContext.DataSetFiles.SingleAsync();
        Assert.Equal(dataFile.DataSetFileId, dataSetFile.Id);
        Assert.Equal(dataFile.Created, dataSetFile.Created);

        var version = await contentDbContext.DataSetFileVersions.SingleAsync();
        Assert.Equal(dataFile.Id, version.Id);
        Assert.Equal(dataFile.DataSetFileId, version.DataSetFileId);
        Assert.Equal(dataFile.DataSetFileVersion, version.Version);
        Assert.Equal(dataFile.SubjectId, version.SubjectId);
        Assert.Equal(dataFile.RootPath, version.RootPath);
        Assert.Equal(dataFile.Filename, version.Filename);
        Assert.Equal(dataFile.ContentType, version.ContentType);
        Assert.Equal(dataFile.ContentLength, version.ContentLength);
        Assert.Equal(metaFile.Id, version.MetaFileId);
        Assert.Equal(metaFile.Filename, version.MetaFilename);
        Assert.Equal(metaFile.ContentType, version.MetaContentType);
        Assert.Equal(metaFile.ContentLength, version.MetaContentLength);
        Assert.Equal(
            JsonConvert.SerializeObject(dataFile.DataSetFileMeta),
            JsonConvert.SerializeObject(version.DataSetFileMeta)
        );
        Assert.Equal(
            JsonConvert.SerializeObject(dataFile.FilterHierarchies),
            JsonConvert.SerializeObject(version.FilterHierarchies)
        );
        Assert.Equal(dataFile.ReplacedById, version.ReplacedById);
        Assert.Equal(dataFile.ReplacingId, version.ReplacingId);
        Assert.Equal(dataFile.Created, version.Created);
        Assert.Equal(dataFile.CreatedById, version.CreatedById);

        var link = await contentDbContext.ReleaseVersionDataSetFileVersions.SingleAsync();
        AssertLinkMirrors(dataReleaseFile, link);
    }

    private static void AssertLinkMirrors(ReleaseFile releaseFile, ReleaseVersionDataSetFileVersion link)
    {
        Assert.Equal(releaseFile.Id, link.Id);
        Assert.Equal(releaseFile.ReleaseVersionId, link.ReleaseVersionId);
        Assert.Equal(releaseFile.FileId, link.DataSetFileVersionId);
        Assert.Equal(releaseFile.Name, link.Name);
        Assert.Equal(releaseFile.Summary, link.Summary);
        Assert.Equal(releaseFile.Order, link.Order);
        Assert.Equal(releaseFile.PublicApiCompatible, link.PublicApiCompatible);
        Assert.Equal(releaseFile.PublicApiDataSetId, link.PublicApiDataSetId);
        Assert.Equal(releaseFile.PublicApiDataSetVersion, link.PublicApiDataSetVersion);
        Assert.Equal(
            JsonConvert.SerializeObject(releaseFile.FilterSequence),
            JsonConvert.SerializeObject(link.FilterSequence)
        );
        Assert.Equal(
            JsonConvert.SerializeObject(releaseFile.IndicatorSequence),
            JsonConvert.SerializeObject(link.IndicatorSequence)
        );
        Assert.Equal(releaseFile.Published, link.Published);
    }
}
