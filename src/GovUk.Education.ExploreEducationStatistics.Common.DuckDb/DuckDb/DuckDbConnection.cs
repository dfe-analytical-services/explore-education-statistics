using DuckDB.NET.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

public class DuckDbConnection(string connectionString = DuckDBConnectionStringBuilder.InMemoryConnectionString)
    : DuckDBConnection(connectionString), IDuckDbConnection
{
    public static DuckDbConnection CreateFileConnection(string filename)
    {
        return new DuckDbConnection($"DataSource={filename}");
    }

    public static DuckDbConnection CreateFileConnectionReadOnly(string filename)
    {
        return new DuckDbConnection($"DataSource={filename};access_mode=read_only");
    }

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

    public async Task<int> ExecuteNonQueryAsync(string commandText)
    {
        await using var command = CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteNonQueryAsync();
    }
}
