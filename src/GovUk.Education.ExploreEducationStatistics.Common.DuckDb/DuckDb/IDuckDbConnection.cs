using System.Data;
using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

public interface IDuckDbConnection : IDbConnection
{
    DuckDBAppender CreateAppender(string table);
}
