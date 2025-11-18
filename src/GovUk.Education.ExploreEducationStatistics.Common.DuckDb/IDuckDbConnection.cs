using System.Data;
using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb;

public interface IDuckDbConnection : IDbConnection
{
    DuckDBAppender CreateAppender(string table);
}
