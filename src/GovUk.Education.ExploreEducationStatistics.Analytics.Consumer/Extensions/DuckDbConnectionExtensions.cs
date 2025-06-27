using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Extensions;

public static class DuckDbConnectionExtensions
{
    /// <summary>
    /// Read JSON files directly into a DuckDb with no additional transformations
    /// or logic.
    /// </summary>
    /// <param name="connection">An open DuckDb connection.</param>
    /// <param name="jsonFilePath">The path to the JSON file to be inserted into the DuckDb table.</param>
    /// <param name="tableName">The name of the DuckDb table into which the JSON will be imported.</param>
    public static async Task DirectCopyJsonIntoDuckDbTable(
        this DuckDbConnection connection,
        string jsonFilePath,
        string tableName)
    {
        await connection.ExecuteNonQueryAsync($@"
            INSERT INTO {tableName} BY NAME (
                SELECT *
                FROM read_json('{jsonFilePath}', 
                    format='unstructured'
                )
             )
        ");
    }
    
    /// <summary>
    /// Copy the contents of a DuckDb table directly into a Parquet file with no
    /// additional transformations or logic.
    /// </summary>
    /// <param name="connection">An open DuckDb connection.</param>
    /// <param name="tableName">The name of the DuckDb table in which the data to copy exists.</param>
    /// <param name="parquetFilePath">The full filepath of the Parquet file to create.</param>
    public static async Task DirectCopyDuckDbTableIntoParquetFile(
        this DuckDbConnection connection,
        string tableName,
        string parquetFilePath)
    {
        await connection.ExecuteNonQueryAsync($@"
            COPY (
                SELECT *
                FROM {tableName}
                ORDER BY startTime ASC
            )
            TO '{parquetFilePath}' (FORMAT 'parquet', CODEC 'zstd')
        ");
    }
}
