using System.Data.Common;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

/// <summary>
/// Wrapper around underlying DuckDB.NET implementation to patch
/// functionality that isn't working correctly.
/// </summary>
public class DuckDbCommand : DuckDB.NET.Data.DuckDBCommand
{
    protected override DbParameter CreateDbParameter() => new DuckDbParameter();
}
