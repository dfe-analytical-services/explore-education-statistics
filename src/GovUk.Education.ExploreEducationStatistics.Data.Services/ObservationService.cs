#nullable enable
using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class ObservationService(
    StatisticsDbContext context,
    IMatchingObservationsQueryGenerator queryGenerator,
    IRawSqlExecutor sqlExecutor,
    ILogger<ObservationService> logger
) : IObservationService
{
    public async Task<ITempTableReference> GetMatchedObservations(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var sw = Stopwatch.StartNew();

        var (sql, sqlParameters, matchingObservationTable) = await queryGenerator.GetMatchingObservationsQuery(
            context,
            query.SubjectId,
            query.GetFilterItemIds(),
            query.LocationIds,
            query.TimePeriod,
            cancellationToken
        );

        // Execute the query to find matching Observation Ids.
        await sqlExecutor.ExecuteSqlRaw(
            context: context,
            sql: sql,
            parameters: sqlParameters,
            cancellationToken: cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Finished fetching {ObservationCount} Observations in a total of {Milliseconds} ms",
                context.MatchedObservations.Count(),
                sw.Elapsed.TotalMilliseconds
            );
        }

        return matchingObservationTable;
    }
}
