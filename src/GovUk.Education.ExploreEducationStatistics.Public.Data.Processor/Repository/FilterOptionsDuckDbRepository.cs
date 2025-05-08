using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class FilterOptionsDuckDbRepository(PublicDataDbContext publicDataDbContext) : IFilterOptionsDuckDbRepository
{
    public async Task CreateFilterOptionsTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.FilterMetas)
            .Query()
            .Include(m => m.Options)
            .LoadAsync(cancellationToken);

        await duckDbConnection.SqlBuilder(
            $"""
             CREATE TABLE {FilterOptionsTable.TableName:raw}(
                 {FilterOptionsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                 {FilterOptionsTable.Cols.Label:raw} VARCHAR,
                 {FilterOptionsTable.Cols.PublicId:raw} VARCHAR,
                 {FilterOptionsTable.Cols.FilterId:raw} VARCHAR,
                 {FilterOptionsTable.Cols.FilterColumn:raw} VARCHAR
             )
             """
        ).ExecuteAsync(cancellationToken: cancellationToken);

        var id = 1;

        foreach (var filter in dataSetVersion.FilterMetas)
        {
            using var appender = duckDbConnection.CreateAppender(table: FilterOptionsTable.TableName);

            foreach (var link in filter.OptionLinks.OrderBy(l => l.Option.Label))
            {
                var insertRow = appender.CreateRow();

                insertRow.AppendValue(id++);
                insertRow.AppendValue(link.Option.Label);
                insertRow.AppendValue(link.PublicId);
                insertRow.AppendValue(filter.PublicId);
                insertRow.AppendValue(filter.Column);

                insertRow.EndRow();
            }
        }
    }
}
