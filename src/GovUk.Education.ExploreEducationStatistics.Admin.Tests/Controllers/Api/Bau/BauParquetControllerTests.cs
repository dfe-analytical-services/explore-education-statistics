#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public abstract class BauParquetControllerTests
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();

    public class ExportDataFileToParquetTests : BauParquetControllerTests
    {
        [Fact]
        public async Task StatsDbDataFile_QueuesExport()
        {
            var file = await SeedDataFile();

            var dataProcessorClient = new Mock<IDataProcessorClient>(Strict);
            dataProcessorClient
                .Setup(c => c.ExportParquet(file.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await BuildController(dataProcessorClient.Object).ExportDataFileToParquet(file.Id);

            VerifyAllMocks(dataProcessorClient);
            Assert.IsType<AcceptedResult>(result);
        }

        [Fact]
        public async Task DataFileNotFound_ReturnsNotFound()
        {
            var result = await BuildController().ExportDataFileToParquet(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DataFileAlreadyHasParquet_ReturnsBadRequest()
        {
            var file = await SeedDataFile(hasParquet: true);

            var result = await BuildController().ExportDataFileToParquet(file.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DataFileNotStoredInStatsDb_ReturnsBadRequest()
        {
            var file = await SeedDataFile(dataStorageVersion: DataStorageVersion.ParquetV1);

            var result = await BuildController().ExportDataFileToParquet(file.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ImportNotComplete_ReturnsBadRequest()
        {
            var file = await SeedDataFile(importStatus: DataImportStatus.STAGE_3);

            var result = await BuildController().ExportDataFileToParquet(file.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    public class ExportDataFilesToParquetTests : BauParquetControllerTests
    {
        [Fact]
        public async Task DryRun_ListsStatsDbDataFilesWithoutParquetAndQueuesNothing()
        {
            var file = await SeedDataFile();
            await SeedDataFile(hasParquet: true);
            await SeedDataFile(dataStorageVersion: DataStorageVersion.ParquetV1);
            await SeedDataFile(importStatus: DataImportStatus.FAILED);

            var result = await BuildController().ExportDataFilesToParquet();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal([file.Id], GetFileIds(okResult));
        }

        [Fact]
        public async Task NotDryRun_QueuesExportForEachDataFile()
        {
            var file1 = await SeedDataFile(created: DateTime.UtcNow.AddDays(-2));
            var file2 = await SeedDataFile(created: DateTime.UtcNow.AddDays(-1));
            await SeedDataFile(created: DateTime.UtcNow);

            var dataProcessorClient = new Mock<IDataProcessorClient>(Strict);
            dataProcessorClient
                .Setup(c => c.ExportParquet(file1.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            dataProcessorClient
                .Setup(c => c.ExportParquet(file2.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await BuildController(dataProcessorClient.Object)
                .ExportDataFilesToParquet(limit: 2, dryRun: false);

            VerifyAllMocks(dataProcessorClient);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal([file1.Id, file2.Id], GetFileIds(okResult));
        }

        private static List<Guid> GetFileIds(OkObjectResult result)
        {
            var fileIds = result.Value!.GetType().GetProperty("FileIds")!.GetValue(result.Value);
            return Assert.IsType<List<Guid>>(fileIds);
        }
    }

    private async Task<File> SeedDataFile(
        DataStorageVersion dataStorageVersion = DataStorageVersion.StatsDB,
        bool hasParquet = false,
        DataImportStatus importStatus = DataImportStatus.COMPLETE,
        DateTime? created = null
    )
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            Filename = "data.csv",
            Type = FileType.Data,
            SubjectId = Guid.NewGuid(),
            DataStorageVersion = dataStorageVersion,
            HasParquet = hasParquet,
            Created = created ?? DateTime.UtcNow,
        };

        var metaFile = new File
        {
            Id = Guid.NewGuid(),
            RootPath = file.RootPath,
            Filename = "data.meta.csv",
            Type = FileType.Metadata,
        };

        await using var contentDbContext = InMemoryApplicationDbContext(_contentDbContextId);

        contentDbContext.Files.AddRange(file, metaFile);
        contentDbContext.DataImports.Add(
            new DataImport
            {
                FileId = file.Id,
                MetaFileId = metaFile.Id,
                SubjectId = file.SubjectId.Value,
                Status = importStatus,
            }
        );
        await contentDbContext.SaveChangesAsync();

        return file;
    }

    private BauParquetController BuildController(IDataProcessorClient? dataProcessorClient = null)
    {
        return new BauParquetController(
            InMemoryApplicationDbContext(_contentDbContextId),
            dataProcessorClient ?? Mock.Of<IDataProcessorClient>(Strict)
        );
    }
}
