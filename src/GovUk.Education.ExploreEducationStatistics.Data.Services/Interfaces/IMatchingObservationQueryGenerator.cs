#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Data.SqlClient;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IMatchingObservationsQueryGenerator
{
    Task<(string, IList<SqlParameter>, ITempTableReference)> GetMatchingObservationsQuery(
        StatisticsDbContext context,
        Guid subjectId,
        IList<Guid> filterItemIds,
        IList<Guid> locationIds,
        TimePeriodQuery? timePeriodQuery,
        CancellationToken cancellationToken
    );
}
