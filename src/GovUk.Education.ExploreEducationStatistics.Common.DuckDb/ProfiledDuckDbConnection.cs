using DuckDB.NET.Data;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb;

/// <summary>
/// DuckDB connection profiled using MiniProfiler.
/// </summary>
public class ProfiledDuckDbConnection(
    string connectionString = DuckDBConnectionStringBuilder.InMemoryConnectionString,
    IDbProfiler? profiler = null
) : ProfiledDbConnection(new DuckDbConnection(connectionString), profiler ?? MiniProfiler.Current), IDuckDbConnection
{
    private DuckDbConnection WrappedDuckDbConnection => (DuckDbConnection)WrappedConnection;

    public DuckDBAppender CreateAppender(string table)
    {
        return WrappedDuckDbConnection.CreateAppender(table);
    }
}
