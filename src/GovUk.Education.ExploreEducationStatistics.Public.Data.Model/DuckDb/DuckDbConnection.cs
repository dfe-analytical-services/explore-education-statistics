using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

public class DuckDbConnection(string connectionString = "DataSource=:memory:")
    : DuckDBConnection(connectionString), IDuckDbConnection
{
    public override DuckDbCommand CreateCommand()
    {
        // Bit rubbish to do this but we don't have access to the
        // underlying `Transaction` so we need to get a reference
        // to it through by creating a base command object first.
        var wrappedCommand = base.CreateCommand();

        return new DuckDbCommand
        {
            Connection = this,
            Transaction = wrappedCommand.Transaction
        };
    }
}