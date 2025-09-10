using System.Data.Common;
using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

/// <summary>
/// Wrapper around underlying DuckDB.NET implementation to patch
/// functionality that isn't working correctly.
/// </summary>
public class DuckDbCommand : DuckDBCommand
{
    protected override DbParameter CreateDbParameter() => new DuckDbParameter();
}
