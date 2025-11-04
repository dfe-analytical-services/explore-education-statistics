using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.DuckDb;

public abstract class DuckDbConstantsTests
{
    public class ReadCsvOptionsTests : DuckDbConstantsTests
    {
        /// <summary>
        /// Test that without our default <see cref="DuckDbConstants.ReadCsvOptions"/>
        /// CSV-reading configuration specified, DuckDB's early optimisations will
        /// cause errors when hitting a CSV line later on in a file that does not
        /// conform to the configuration that it has optimised itself with.
        ///
        /// In this case, we are testing that hitting a quote-delimited cell far enough
        /// down in a data set will cause a failure, as by that point DuckDB has
        /// determined that cells are not quote-delimited.
        /// </summary>
        [Fact]
        public async Task DuckDb_EarlyOptimisations_ErrorsWithoutCsvReadingQuoteConfig()
        {
            var duckDbConnection = new DuckDbConnection();

            var csvReadingOptionsWithoutQuoteConfig = DuckDbConstants.ReadCsvOptions.Replace("""QUOTE = '"',""", "");

            var readCsvCommand = duckDbConnection.SqlBuilder(
                $"""
                SELECT COUNT(*) FROM read_csv(
                    '{ProcessorTestData.LargeDataSet.CsvDataGzipFilePath:raw}',
                    {csvReadingOptionsWithoutQuoteConfig:raw}
                )
                """
            );

            var exception = await Assert.ThrowsAsync<DuckDBException>(() => readCsvCommand.ExecuteScalarAsync<int>());

            Assert.Contains("Invalid Input Error: CSV Error on Line: 20481", exception.Message);
            Assert.Contains("Expected Number of Columns: 18 Found: 19", exception.Message);
        }

        /// <summary>
        /// Test that with our default <see cref="DuckDbConstants.ReadCsvOptions"/>
        /// CSV-reading configuration explicitly specified, DuckDB's early optimisations
        /// will now no longer cause errors.
        ///
        /// In this case, we are testing that hitting a quote-delimited cell far enough
        /// down in a data set will no longer cause a failure, because we have been explicit
        /// in how our delimiters are set up.
        /// </summary>
        [Fact]
        public async Task DuckDb_EarlyOptimisations_SuccessWithDefaultCsvReadingConfig()
        {
            var duckDbConnection = new DuckDbConnection();

            var readCsvCommand = duckDbConnection.SqlBuilder(
                $"""
                SELECT COUNT(*) FROM read_csv(
                    '{ProcessorTestData.LargeDataSet.CsvDataGzipFilePath:raw}',
                    {DuckDbConstants.ReadCsvOptions:raw}
                )
                """
            );

            var result = await readCsvCommand.ExecuteScalarAsync<int>();
            Assert.Equal(20499, result);
        }
    }
}
