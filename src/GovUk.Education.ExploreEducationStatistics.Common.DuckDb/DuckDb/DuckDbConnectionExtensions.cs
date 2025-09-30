using System.Runtime.CompilerServices;
using InterpolatedSql;
using InterpolatedSql.SqlBuilders;

namespace GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

public static class DuckDbConnectionExtensions
{
    public static DuckDbDapperSqlBuilder SqlBuilder(this IDuckDbConnection connection)
    {
        return new DuckDbDapperSqlBuilder(connection);
    }

    public static DuckDbDapperSqlBuilder SqlBuilder(
        this IDuckDbConnection connection,
        FormattableString command,
        InterpolatedSqlBuilderOptions? options = null
    )
    {
        return new DuckDbDapperSqlBuilder(connection, command, options);
    }

    public static DuckDbDapperSqlBuilder SqlBuilder(
        this IDuckDbConnection connection,
        string command,
        InterpolatedSqlBuilderOptions? options = null
    )
    {
        return new DuckDbDapperSqlBuilder(connection, command, options);
    }

    public static DuckDbDapperSqlBuilder SqlBuilder(
        this IDuckDbConnection connection,
        InterpolatedSqlBuilderOptions options
    )
    {
        return new DuckDbDapperSqlBuilder(connection, options);
    }

    public static DuckDbDapperSqlBuilder SqlBuilder(
        this IDuckDbConnection connection,
        ref InterpolatedSqlHandler command
    )
    {
        if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
        {
            command.AdjustMultilineString();
        }

        return new DuckDbDapperSqlBuilder(
            connection,
            command.InterpolatedSqlBuilder.AsFormattableString()
        );
    }

    public static DuckDbDapperSqlBuilder SqlBuilder(
        this IDuckDbConnection connection,
        InterpolatedSqlBuilderOptions options,
        [InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler command
    )
    {
        if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
        {
            command.AdjustMultilineString();
        }

        return new DuckDbDapperSqlBuilder(
            connection,
            command.InterpolatedSqlBuilder.AsFormattableString(),
            options
        );
    }
}
