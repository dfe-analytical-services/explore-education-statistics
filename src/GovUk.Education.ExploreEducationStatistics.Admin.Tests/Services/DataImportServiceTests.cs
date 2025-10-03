#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataImportServiceTests
{
    [Fact]
    public async Task CancelImport()
    {
        var releaseVersion = new ReleaseVersion();

        var file = new File { Type = FileType.Data };

        var import = new DataImport { File = file };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(
                new ReleaseFile { ReleaseVersion = releaseVersion, File = file }
            );
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var dataProcessorClient = new Mock<IDataProcessorClient>(Strict);
        var releaseFileService = new Mock<IReleaseFileService>(Strict);
        var userService = new Mock<IUserService>(Strict);

        dataProcessorClient.Setup(s => s.CancelImport(import.Id, CancellationToken.None)).Returns(Task.CompletedTask);

        releaseFileService.Setup(s => s.CheckFileExists(releaseVersion.Id, file.Id, FileType.Data)).ReturnsAsync(file);

        userService
            .Setup(s => s.MatchesPolicy(It.Is<File>(f => f.Id == file.Id), SecurityPolicies.CanCancelOngoingImports))
            .ReturnsAsync(true);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(
                contentDbContext: contentDbContext,
                releaseFileService: releaseFileService.Object,
                dataProcessorClient: dataProcessorClient.Object,
                userService: userService.Object
            );

            var result = await service.CancelImport(releaseVersionId: releaseVersion.Id, fileId: file.Id);

            MockUtils.VerifyAllMocks(releaseFileService, userService, dataProcessorClient);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task CancelFileImportButNotAllowed()
    {
        var releaseVersion = new ReleaseVersion();

        var file = new File { Type = FileType.Data };

        var import = new DataImport { File = file };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(
                new ReleaseFile { ReleaseVersion = releaseVersion, File = file }
            );
            await contentDbContext.DataImports.AddAsync(import);
            await contentDbContext.SaveChangesAsync();
        }

        var releaseFileService = new Mock<IReleaseFileService>(Strict);
        var userService = new Mock<IUserService>(Strict);

        releaseFileService.Setup(s => s.CheckFileExists(releaseVersion.Id, file.Id, FileType.Data)).ReturnsAsync(file);

        userService
            .Setup(s => s.MatchesPolicy(It.Is<File>(f => f.Id == file.Id), SecurityPolicies.CanCancelOngoingImports))
            .ReturnsAsync(false);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(
                contentDbContext: contentDbContext,
                releaseFileService: releaseFileService.Object,
                userService: userService.Object
            );

            var result = await service.CancelImport(releaseVersionId: releaseVersion.Id, fileId: file.Id);

            MockUtils.VerifyAllMocks(releaseFileService, userService);

            result.AssertForbidden();
        }
    }

    [Fact]
    public async Task HasIncompleteImports_NoReleaseFiles()
    {
        var releaseVersion1 = new ReleaseVersion();
        var releaseVersion2 = new ReleaseVersion();

        var release2File1 = new File { Type = FileType.Data };

        // Incomplete imports for other Releases should be ignored

        var release2Import1 = new DataImport { File = release2File1, Status = DataImportStatus.STAGE_1 };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(
                new ReleaseFile { ReleaseVersion = releaseVersion2, File = release2File1 }
            );
            await contentDbContext.DataImports.AddRangeAsync(release2Import1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(contentDbContext: contentDbContext);

            var result = await service.HasIncompleteImports(releaseVersion1.Id);
            Assert.False(result);
        }
    }

    [Fact]
    public async Task HasIncompleteImports_ReleaseHasCompletedImports()
    {
        var release1 = new ReleaseVersion();
        var release2 = new ReleaseVersion();

        var release1File1 = new File { Type = FileType.Data };

        var release1File2 = new File { Type = FileType.Data };

        var release2File1 = new File { Type = FileType.Data };

        var release1Import1 = new DataImport { File = release1File1, Status = DataImportStatus.COMPLETE };

        var release1Import2 = new DataImport { File = release1File2, Status = DataImportStatus.COMPLETE };

        // Incomplete imports for other Releases should be ignored

        var release2Import1 = new DataImport { File = release2File1, Status = DataImportStatus.STAGE_1 };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(
                new ReleaseFile { ReleaseVersion = release1, File = release1File1 },
                new ReleaseFile { ReleaseVersion = release1, File = release1File2 },
                new ReleaseFile { ReleaseVersion = release2, File = release2File1 }
            );
            await contentDbContext.DataImports.AddRangeAsync(release1Import1, release1Import2, release2Import1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(contentDbContext: contentDbContext);

            var result = await service.HasIncompleteImports(release1.Id);
            Assert.False(result);
        }
    }

    [Fact]
    public async Task HasIncompleteImports_ReleaseHasIncompleteImports()
    {
        var releaseVersion = new ReleaseVersion();

        var file1 = new File { Type = FileType.Data };

        var file2 = new File { Type = FileType.Data };

        var import1 = new DataImport { File = file1, Status = DataImportStatus.COMPLETE };

        var import2 = new DataImport { File = file2, Status = DataImportStatus.STAGE_1 };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(
                new ReleaseFile { ReleaseVersion = releaseVersion, File = file1 },
                new ReleaseFile { ReleaseVersion = releaseVersion, File = file2 }
            );
            await contentDbContext.DataImports.AddRangeAsync(import1, import2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(contentDbContext: contentDbContext);

            var result = await service.HasIncompleteImports(releaseVersion.Id);
            Assert.True(result);
        }
    }

    [Fact]
    public async Task Import()
    {
        var subjectId = Guid.NewGuid();

        var sourceFile = new File
        {
            Filename = "data.zip",
            Type = FileType.DataZip,
            SubjectId = null,
        };

        var dataFile = new File
        {
            Filename = "data.csv",
            Type = FileType.Data,
            SubjectId = subjectId,
            Source = sourceFile,
        };

        var metaFile = new File
        {
            Filename = "data.meta.csv",
            Type = FileType.Metadata,
            SubjectId = subjectId,
            Source = sourceFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddRangeAsync(sourceFile, dataFile, metaFile);
        }

        var dataProcessorClient = new Mock<IDataProcessorClient>(Strict);

        dataProcessorClient.Setup(s => s.Import(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataImportService(
                contentDbContext: contentDbContext,
                dataProcessorClient: dataProcessorClient.Object
            );

            var result = await service.Import(subjectId, dataFile, metaFile, sourceFile);

            MockUtils.VerifyAllMocks(dataProcessorClient);

            Assert.Equal(dataFile.Id, result.FileId);
            Assert.Equal(metaFile.Id, result.MetaFileId);
            Assert.Equal(subjectId, result.SubjectId);
            Assert.Equal(DataImportStatus.QUEUED, result.Status);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.Equal(dataFile.SourceId, result.ZipFileId);
        }
    }

    private static DataImportService BuildDataImportService(
        ContentDbContext contentDbContext,
        IDataImportRepository? dataImportRepository = null,
        IDataProcessorClient? dataProcessorClient = null,
        IReleaseFileService? releaseFileService = null,
        IUserService? userService = null
    )
    {
        return new DataImportService(
            contentDbContext,
            dataImportRepository ?? new DataImportRepository(contentDbContext),
            dataProcessorClient ?? Mock.Of<IDataProcessorClient>(Strict),
            releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object
        );
    }
}
