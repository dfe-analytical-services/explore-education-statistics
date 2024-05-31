using DuckDB.NET.Data;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

/// <summary>
/// DuckDB connection profiled using MiniProfiler.
/// </summary>
public class ProfiledDuckDbConnection(string connectionString = "DataSource=:memory:", IDbProfiler? profiler = null)
    : ProfiledDbConnection(new DuckDbConnection(connectionString), profiler ?? MiniProfiler.Current), IDuckDbConnection
{
    private DuckDbConnection WrappedDuckDbConnection => (DuckDbConnection)WrappedConnection;

    public DuckDBAppender CreateAppender(string table)
    {
        return WrappedDuckDbConnection.CreateAppender(table);
    }
}
