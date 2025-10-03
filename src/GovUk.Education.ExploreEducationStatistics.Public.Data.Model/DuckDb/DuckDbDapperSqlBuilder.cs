using System.Data;
using System.Text;
using InterpolatedSql;
using InterpolatedSql.Dapper;
using InterpolatedSql.Dapper.SqlBuilders;
using InterpolatedSql.SqlBuilders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

public class DuckDbDapperSqlBuilder : DuckDbSqlBuilder, IDapperSqlBuilder
{
    public DuckDbDapperSqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null)
        : base(options)
    {
        DbConnection = connection;
    }

    public DuckDbDapperSqlBuilder(IDbConnection connection, FormattableString value)
        : base(value)
    {
        DbConnection = connection;
    }

    public DuckDbDapperSqlBuilder(
        IDbConnection connection,
        FormattableString value,
        InterpolatedSqlBuilderOptions? options = null
    )
        : base(value, options)
    {
        DbConnection = connection;
    }

    public DuckDbDapperSqlBuilder(IDbConnection connection, string value, InterpolatedSqlBuilderOptions? options = null)
        : base(value, options)
    {
        DbConnection = connection;
    }

    protected internal DuckDbDapperSqlBuilder(
        IDbConnection connection,
        InterpolatedSqlBuilderOptions? options,
        StringBuilder? format,
        List<InterpolatedSqlParameter>? arguments
    )
        : base(options, format, arguments)
    {
        DbConnection = connection;
    }

    public override IDapperSqlCommand Build() => ToDapperSqlCommand();

    public IDapperSqlCommand ToDapperSqlCommand()
    {
        var format = _format.ToString();

        return new ImmutableDapperCommand(
            this.DbConnection!,
            BuildSql(format, _sqlParameters),
            format,
            _sqlParameters,
            _explicitParameters
        );
    }
}
