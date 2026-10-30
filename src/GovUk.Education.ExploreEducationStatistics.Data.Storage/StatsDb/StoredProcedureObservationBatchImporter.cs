#nullable enable
using System.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class StoredProcedureObservationBatchImporter : IObservationBatchImporter
{
    public async Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations)
    {
        var observationsTable = new DataTable();
        observationsTable.Columns.Add("Id", typeof(Guid));
        observationsTable.Columns.Add("SubjectId", typeof(Guid));
        observationsTable.Columns.Add("LocationId", typeof(Guid));
        observationsTable.Columns.Add("Year", typeof(int));
        observationsTable.Columns.Add("TimeIdentifier", typeof(string));
        observationsTable.Columns.Add("Measures", typeof(string));
        observationsTable.Columns.Add("CsvRow", typeof(long));

        var observationsFilterItemsTable = new DataTable();
        observationsFilterItemsTable.Columns.Add("ObservationId", typeof(Guid));
        observationsFilterItemsTable.Columns.Add("FilterItemId", typeof(Guid));
        observationsFilterItemsTable.Columns.Add("FilterId", typeof(Guid));

        foreach (var o in observations)
        {
            observationsTable.Rows.Add(
                o.Id,
                o.SubjectId,
                o.LocationId,
                o.Year,
                o.TimeIdentifier.GetEnumValue(),
                "{" + string.Join(",", o.Measures.Select(x => $"\"{x.Key}\":\"{x.Value}\"")) + "}",
                o.CsvRow
            );

            foreach (var item in o.FilterItems)
            {
                observationsFilterItemsTable.Rows.Add(item.ObservationId, item.FilterItemId, item.FilterId);
            }
        }

        var parameter = new SqlParameter("@Observations", SqlDbType.Structured)
        {
            Value = observationsTable,
            TypeName = "[dbo].[ObservationType]",
        };

        await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[InsertObservations] @Observations", parameter);

        parameter = new SqlParameter("@ObservationFilterItems", SqlDbType.Structured)
        {
            Value = observationsFilterItemsTable,
            TypeName = "[dbo].[ObservationFilterItemType]",
        };

        await context.Database.ExecuteSqlRawAsync(
            "EXEC [dbo].[InsertObservationFilterItems] @ObservationFilterItems",
            parameter
        );
    }
}
