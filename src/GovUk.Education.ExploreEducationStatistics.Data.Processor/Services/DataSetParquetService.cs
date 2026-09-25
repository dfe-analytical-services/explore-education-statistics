#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DataSetParquetService(
    ILogger<DataSetParquetService> logger,
    IPrivateBlobStorageService privateBlobStorageService,
    IDataFilesPathResolver dataFilesPathResolver,
    IDbContextSupplier dbContextSupplier
) : IDataSetParquetService
{
    public async Task WriteParquetV1File(DataImport import)
    {
        logger.LogInformation("Writing Parquet file for {Filename}", import.File.Filename);

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"ees-parquet-{import.Id}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var csvPath = Path.Combine(tempDirectory, "data.csv");

            await using (var blobStream = await privateBlobStorageService.GetDataFileStreamProvider(import)())
            await using (var csvStream = new FileStream(csvPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await blobStream.CopyToAsync(csvStream);
            }

            var parquetPath = dataFilesPathResolver.ParquetV1Path(import.File);
            Directory.CreateDirectory(Path.GetDirectoryName(parquetPath)!);

            await using (var duckDbConnection = new DuckDbConnection())
            {
                await duckDbConnection.OpenAsync();

                // ALL_VARCHAR keeps every column as text so the Parquet file mirrors the CSV exactly,
                // rather than DuckDB inferring numeric/date types from the values.
                await duckDbConnection.ExecuteNonQueryAsync(
                    $"""
                    COPY (
                        SELECT *
                        FROM read_csv('{csvPath}', ALL_VARCHAR = true, HEADER = true, QUOTE = '"', DELIM = ',')
                    )
                    TO '{parquetPath}' (FORMAT PARQUET, CODEC ZSTD)
                    """
                );
            }

            await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
            var file = await contentDbContext.Files.SingleAsync(f => f.Id == import.FileId);
            file.HasParquet = true;
            await contentDbContext.SaveChangesAsync();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
