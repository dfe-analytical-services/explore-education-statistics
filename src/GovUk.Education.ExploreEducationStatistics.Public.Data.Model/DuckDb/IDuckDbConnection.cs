using System.Data;
using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

public interface IDuckDbConnection : IDbConnection
{
    DuckDBAppender CreateAppender(string table);
}
