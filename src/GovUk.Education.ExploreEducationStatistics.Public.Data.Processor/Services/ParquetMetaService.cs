using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class ParquetMetaService(
    IDuckDbConnection duckDb,
    PublicDataDbContext publicDataDbContext) : IParquetMetaService
{
    public async Task CreateParquetDataSetMetaTables(Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        // Create temporary meta tables in DuckDB to allow us to do data transform
        // in DuckDB itself (i.e. changing all the filters and locations to normalised IDs).
        // Trying to transform the data via using Appender API in C# is slower
        // and seems to regularly cause DuckDB crashes for larger data sets.

        // TODO EES-5097 Check all of these includes are necessary
        var dataSetVersion = await publicDataDbContext
            .DataSetVersions
            .AsSplitQuery()
            .Include(v => v.FilterMetas)
            .ThenInclude(m => m.Options)
            .Include(v => v.IndicatorMetas)
            .Include(v => v.LocationMetas)
            .ThenInclude(m => m.Options)
            .Include(v => v.TimePeriodMetas)
            .FirstAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken);

        await CreateParquetIndicatorTable(dataSetVersion);
        await CreateParquetLocationMetaTable(dataSetVersion);
        await CreateParquetFilterMetaTable(dataSetVersion);
        await CreateParquetTimePeriodMetaTable(dataSetVersion);
    }

    private async Task CreateParquetIndicatorTable(DataSetVersion version)
    {
        await duckDb.ExecuteAsync(
            $"""
             CREATE TABLE {IndicatorsTable.TableName}(
                 {IndicatorsTable.Cols.Id} VARCHAR PRIMARY KEY,
                 {IndicatorsTable.Cols.Label} VARCHAR,
                 {IndicatorsTable.Cols.Unit} VARCHAR,
                 {IndicatorsTable.Cols.DecimalPlaces} TINYINT,
             )
             """
        );

        using var appender = duckDb.CreateAppender(table: IndicatorsTable.TableName);

        foreach (var meta in version.IndicatorMetas)
        {
            var insertRow = appender.CreateRow();

            insertRow.AppendValue(meta.PublicId);
            insertRow.AppendValue(meta.Label);
            insertRow.AppendValue(meta.Unit?.GetEnumLabel() ?? string.Empty);
            insertRow.AppendValue(meta.DecimalPlaces);
            insertRow.EndRow();
        }
    }

    private async Task CreateParquetLocationMetaTable(DataSetVersion version)
    {
        await duckDb.ExecuteAsync(
            $"""
             CREATE TABLE {LocationOptionsTable.TableName}(
                 {LocationOptionsTable.Cols.Id} INTEGER PRIMARY KEY,
                 {LocationOptionsTable.Cols.Label} VARCHAR,
                 {LocationOptionsTable.Cols.Level} VARCHAR,
                 {LocationOptionsTable.Cols.PublicId} VARCHAR,
                 {LocationOptionsTable.Cols.Code} VARCHAR,
                 {LocationOptionsTable.Cols.OldCode} VARCHAR,
                 {LocationOptionsTable.Cols.Urn} VARCHAR,
                 {LocationOptionsTable.Cols.LaEstab} VARCHAR,
                 {LocationOptionsTable.Cols.Ukprn} VARCHAR
             )
             """
        );

        var id = 1;

        foreach (var location in version.LocationMetas)
        {
            using var appender = duckDb.CreateAppender(table: LocationOptionsTable.TableName);

            var insertRow = appender.CreateRow();

            foreach (var link in location.OptionLinks.OrderBy(l => l.Option.Label))
            {
                var option = link.Option;

                insertRow.AppendValue(id++);
                insertRow.AppendValue(option.Label);
                insertRow.AppendValue(location.Level.GetEnumValue());
                insertRow.AppendValue(option.PublicId);

                switch (option)
                {
                    case LocationLocalAuthorityOptionMeta laOption:
                        insertRow.AppendValue(laOption.Code);
                        insertRow.AppendValue(laOption.OldCode);
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        break;
                    case LocationCodedOptionMeta codedOption:
                        insertRow.AppendValue(codedOption.Code);
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        break;
                    case LocationProviderOptionMeta providerOption:
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendValue(providerOption.Ukprn);
                        break;
                    case LocationSchoolOptionMeta schoolOption:
                        insertRow.AppendNullValue();
                        insertRow.AppendNullValue();
                        insertRow.AppendValue(schoolOption.Urn);
                        insertRow.AppendValue(schoolOption.LaEstab);
                        insertRow.AppendNullValue();
                        break;
                }

                insertRow.EndRow();
            }
        }
    }

    private async Task CreateParquetFilterMetaTable(DataSetVersion version)
    {
        await duckDb.ExecuteAsync(
            $"""
             CREATE TABLE {FilterOptionsTable.TableName}(
                 {FilterOptionsTable.Cols.Id} INTEGER PRIMARY KEY,
                 {FilterOptionsTable.Cols.Label} VARCHAR,
                 {FilterOptionsTable.Cols.PublicId} VARCHAR,
                 {FilterOptionsTable.Cols.FilterId} VARCHAR
             )
             """
        );

        var id = 1;

        foreach (var filter in version.FilterMetas)
        {
            using var appender = duckDb.CreateAppender(table: FilterOptionsTable.TableName);

            foreach (var link in filter.OptionLinks.OrderBy(l => l.Option.Label))
            {
                var insertRow = appender.CreateRow();

                insertRow.AppendValue(id++);
                insertRow.AppendValue(link.Option.Label);
                insertRow.AppendValue(link.PublicId);
                insertRow.AppendValue(filter.PublicId);

                insertRow.EndRow();
            }
        }
    }

    private async Task CreateParquetTimePeriodMetaTable(DataSetVersion version)
    {
        await duckDb.ExecuteAsync(
            $"""
             CREATE TABLE {TimePeriodsTable.TableName}(
                 {TimePeriodsTable.Cols.Id} INTEGER PRIMARY KEY,
                 {TimePeriodsTable.Cols.Period} VARCHAR,
                 {TimePeriodsTable.Cols.Identifier} VARCHAR
             )
             """
        );

        using var appender = duckDb.CreateAppender(table: TimePeriodsTable.TableName);

        var timePeriods = version.TimePeriodMetas
            .OrderBy(tp => tp.Period)
            .ThenBy(tp => tp.Code);

        var id = 1;

        foreach (var timePeriod in timePeriods)
        {
            var insertRow = appender.CreateRow();

            insertRow.AppendValue(id++);
            insertRow.AppendValue(TimePeriodFormatter.FormatToCsv(timePeriod.Period));
            insertRow.AppendValue(timePeriod.Code.GetEnumLabel());
            insertRow.EndRow();
        }
    }
}
