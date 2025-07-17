using System.Data.Common;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

/// <summary>
/// Wrapper around underlying DuckDB.NET implementation to patch
/// functionality that isn't working correctly.
/// </summary>
public class DuckDbCommand : DuckDB.NET.Data.DuckDBCommand
{
    protected override DbParameter CreateDbParameter() => new DuckDbParameter();
}
