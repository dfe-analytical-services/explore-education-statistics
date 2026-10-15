#nullable enable
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

/// <summary>
/// Checks that the parquet files can be read by the Public API's queries without changes, by comparing them with
/// files written by the Public API's data processor.
/// </summary>
public abstract class PublicApiParquetCompatibilityTests : IDisposable
{
    private static readonly string PublicApiDataFilesPath = Path.Combine(
        PathUtils.ProjectRootPath,
        "src",
        "GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests",
        "Resources",
        "DataFiles",
        "AbsenceSchool"
    );

    private readonly ParquetTestDataSet _testDataSet = new();

    private IParquetDataSetPathResolver PathResolver => _testDataSet.PathResolver;

    public void Dispose() => _testDataSet.Dispose();

    public class SchemaTests : PublicApiParquetCompatibilityTests
    {
        [Theory]
        [InlineData(FilterOptionsTable.ParquetFile)]
        [InlineData(IndicatorsTable.ParquetFile)]
        [InlineData(LocationOptionsTable.ParquetFile)]
        [InlineData(TimePeriodsTable.ParquetFile)]
        public async Task MetaTable_StartsWithPublicApiColumns(string parquetFile)
        {
            await _testDataSet.Write();

            var publicApiColumns = await DescribeColumns(Path.Combine(PublicApiDataFilesPath, parquetFile));
            var columns = await DescribeColumns(
                Path.Combine(PathResolver.DirectoryPath(_testDataSet.SubjectId), parquetFile)
            );

            Assert.Equal(publicApiColumns, columns.Take(publicApiColumns.Count));
        }

        [Fact]
        public async Task DataTable_HasPublicApiColumnsInPublicApiOrder()
        {
            await _testDataSet.Write();

            var publicApiColumns = await DescribeColumns(Path.Combine(PublicApiDataFilesPath, DataTable.ParquetFile));
            var columns = await DescribeColumns(PathResolver.DataPath(_testDataSet.SubjectId));

            // The fixed columns are followed by location, filter and indicator columns, which vary by data set.
            Assert.Equal(publicApiColumns.Take(3), columns.Take(3));
            Assert.Equal(
                [
                    ("locations_la_id", "INTEGER"),
                    ("locations_nat_id", "INTEGER"),
                    ("locations_reg_id", "INTEGER"),
                    ("locations_sch_id", "INTEGER"),
                    ("ncyear", "INTEGER"),
                    ("school_type", "INTEGER"),
                    ("enrolments", "VARCHAR"),
                    ("notes", "VARCHAR"),
                    ("sess_authorised_percent", "VARCHAR"),
                    ("ees_observation_id", "UUID"),
                    ("ees_location_id", "UUID"),
                    ("ees_csv_row", "BIGINT"),
                ],
                columns.Skip(3)
            );

            Assert.All(
                publicApiColumns.Where(column => column.Name.StartsWith("locations_")),
                column => Assert.Equal("INTEGER", column.Type)
            );
        }
    }

    public class QueryTests : PublicApiParquetCompatibilityTests
    {
        [Fact]
        public async Task FilterColumnsByPublicFilterId_AsQueriedByPublicApi()
        {
            await _testDataSet.Write();

            var filterColumns = await Query<(string FilterId, string FilterColumn)>(
                $"""
                SELECT DISTINCT filter_id, filter_column
                FROM '{PathResolver.FilterOptionsPath(_testDataSet.SubjectId)}'
                ORDER BY filter_column
                """
            );

            Assert.Equal(["ncyear", "school_type"], filterColumns.Select(column => column.FilterColumn));
            Assert.Equal(2, filterColumns.Select(column => column.FilterId).Distinct().Count());
        }

        [Fact]
        public async Task DataRowsByOptionIds_AsQueriedByPublicApi()
        {
            await _testDataSet.Write();

            var subjectId = _testDataSet.SubjectId;

            var primaryOptionId = await QuerySingle<int>(
                $"""
                SELECT id FROM '{PathResolver.FilterOptionsPath(subjectId)}'
                WHERE ees_filter_item_id = '{_testDataSet.Primary.Id}'
                """
            );

            var englandOptionId = await QuerySingle<int>(
                $"""
                SELECT id FROM '{PathResolver.LocationOptionsPath(subjectId)}'
                WHERE level = 'NAT' AND code = 'E92000001'
                """
            );

            // The Public API's deferred join to page through data rows.
            var observationIds = await Query<Guid>(
                $"""
                WITH data_ids AS (
                    SELECT data.id
                    FROM '{PathResolver.DataPath(subjectId)}' AS data
                    WHERE data."school_type" = {primaryOptionId}
                    AND data.locations_nat_id = {englandOptionId}
                    AND data.geographic_level = 'National'
                    ORDER BY data.time_period_id DESC, data.id ASC
                    LIMIT 10 OFFSET 0
                )
                SELECT data.ees_observation_id
                FROM '{PathResolver.DataPath(subjectId)}' AS data
                JOIN data_ids ON data_ids.id = data.id
                ORDER BY data.time_period_id DESC, data.id ASC
                """
            );

            Assert.Equal([_testDataSet.Observations[1].Id, _testDataSet.Observations[0].Id], observationIds);
        }
    }

    private static async Task<List<(string Name, string Type)>> DescribeColumns(string path)
    {
        return await Query<(string Name, string Type)>(
            $"SELECT column_name, column_type FROM (DESCRIBE SELECT * FROM '{path}')"
        );
    }

    private static async Task<List<T>> Query<T>(string sql)
    {
        await using var connection = new DuckDbConnection();
        connection.Open();
        return (await connection.QueryAsync<T>(sql)).AsList();
    }

    private static async Task<T> QuerySingle<T>(string sql)
    {
        await using var connection = new DuckDbConnection();
        connection.Open();
        return await connection.QuerySingleAsync<T>(sql);
    }
}
