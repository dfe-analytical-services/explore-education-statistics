using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

/// <summary>
/// Wrapper around underlying DuckDB.NET implementation to patch
/// functionality that isn't working correctly.
/// </summary>
public class DuckDbParameter : DuckDBParameter
{
    public override string ParameterName
    {
        // Hack to prevent `ParameterName` being set. We need to do this as a workaround because
        // named parameters (e.g. $param) don't seem to work correctly in all environments:
        // https://github.com/Giorgi/DuckDB.NET/issues/178
        // Unfortunately, this makes the library pretty incompatible with Dapper which
        // relies heavily on auto-generated named parameters (e.g. $p0, $p1, $p2).
        // By not setting `ParameterName`, we go force the usage of auto-incrementing
        // positional parameters (i.e. ?) which seem to work reliably.
        get => string.Empty;
        set => base.ParameterName = string.Empty;
    }
}
