using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

public class RawSqlExecutor : IRawSqlExecutor
{
    public async Task ExecuteSqlRaw<TDbContext>(
        TDbContext context,
        string sql,
        IList<SqlParameter> parameters,
        CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        await context
            .Database
            .ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}

public interface IRawSqlExecutor
{
    Task ExecuteSqlRaw<TDbContext>(
        TDbContext context,
        string sql,
        IList<SqlParameter> parameters,
        CancellationToken cancellationToken)
        where TDbContext : DbContext;
}
