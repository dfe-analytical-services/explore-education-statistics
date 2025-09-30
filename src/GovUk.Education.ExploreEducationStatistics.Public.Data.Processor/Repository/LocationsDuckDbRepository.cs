using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class LocationsDuckDbRepository(PublicDataDbContext publicDataDbContext)
    : ILocationsDuckDbRepository
{
    public async Task CreateLocationsTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.LocationMetas)
            .Query()
            .Include(m => m.Options)
            .LoadAsync(cancellationToken);

        await duckDbConnection
            .SqlBuilder(
                $"""
                CREATE TABLE {LocationOptionsTable.TableName:raw}(
                    {LocationOptionsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                    {LocationOptionsTable.Cols.Label:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Level:raw} VARCHAR,
                    {LocationOptionsTable.Cols.PublicId:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Code:raw} VARCHAR,
                    {LocationOptionsTable.Cols.OldCode:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Urn:raw} VARCHAR,
                    {LocationOptionsTable.Cols.LaEstab:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Ukprn:raw} VARCHAR
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        var id = 1;

        foreach (var location in dataSetVersion.LocationMetas)
        {
            using var appender = duckDbConnection.CreateAppender(
                table: LocationOptionsTable.TableName
            );

            foreach (var link in location.OptionLinks.OrderBy(l => l.Option.Label))
            {
                var option = link.Option;

                var insertRow = appender.CreateRow();

                insertRow.AppendValue(id++);
                insertRow.AppendValue(option.Label);
                insertRow.AppendValue(location.Level.GetEnumValue());
                insertRow.AppendValue(link.PublicId);

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
}
