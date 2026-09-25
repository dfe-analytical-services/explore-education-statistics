using System.Reflection;
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class DataSetParquetServiceTests
{
    [Fact]
    public async Task WriteParquetV1File()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            Filename = "small-csv.csv",
            Type = FileType.Data,
        };

        var import = new DataImport
        {
            Id = Guid.NewGuid(),
            FileId = file.Id,
            File = file,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Files.Add(file);
            await contentDbContext.SaveChangesAsync();
        }

        var csvPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources",
            file.Filename
        );

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        privateBlobStorageService.SetupGetDownloadStreamWithFilePath(PrivateReleaseFiles, file.Path(), csvPath);

        var dataFilesDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var parquetPath = Path.Combine(dataFilesDirectory, "data", $"{file.Id}.parquet");

        var dataFilesPathResolver = new Mock<IDataFilesPathResolver>(Strict);
        dataFilesPathResolver.Setup(s => s.ParquetV1Path(file)).Returns(parquetPath);

        var service = new DataSetParquetService(
            Mock.Of<ILogger<DataSetParquetService>>(),
            privateBlobStorageService.Object,
            dataFilesPathResolver.Object,
            new InMemoryDbContextSupplier(contentDbContextId: contentDbContextId)
        );

        try
        {
            await service.WriteParquetV1File(import);

            VerifyAllMocks(privateBlobStorageService, dataFilesPathResolver);

            Assert.True(System.IO.File.Exists(parquetPath));

            var csvLines = await System.IO.File.ReadAllLinesAsync(csvPath);
            var expectedColumns = csvLines[0].Split(',');
            var expectedRows = csvLines.Skip(1).Select(line => line.Split(',')).ToList();

            await using (var duckDbConnection = new DuckDbConnection())
            {
                var parquetRows = (await duckDbConnection.QueryAsync($"SELECT * FROM read_parquet('{parquetPath}')"))
                    .Cast<IDictionary<string, object>>()
                    .ToList();

                Assert.Equal(expectedRows.Count, parquetRows.Count);

                foreach (var (expectedRow, parquetRow) in expectedRows.Zip(parquetRows))
                {
                    Assert.Equal(expectedColumns, parquetRow.Keys);
                    Assert.Equal(expectedRow, parquetRow.Values.Select(value => (string)value));
                }
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedFile = await contentDbContext.Files.SingleAsync(f => f.Id == file.Id);
                Assert.True(updatedFile.HasParquet);
                Assert.Equal(DataStorageVersion.StatsDB, updatedFile.DataStorageVersion);
            }
        }
        finally
        {
            Directory.Delete(dataFilesDirectory, recursive: true);
        }
    }
}
