using System.Collections;
using System.Text;
using InterpolatedSql;
using InterpolatedSql.SqlBuilders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

public class DuckDbSqlBuilder : InterpolatedSqlBuilderBase<DuckDbSqlBuilder, IInterpolatedSql>
{
    private static readonly InterpolatedSqlBuilderOptions DefaultOptions = new()
    {
        // Use auto-incrementing positional parameters (i.e. ?), to avoid issues
        // with named parameters which don't seem to work reliably in all environments.
        // See: https://github.com/Giorgi/DuckDB.NET/issues/178
        DatabaseParameterSymbol = "?",
        CalculateAutoParameterName = (_, _) => string.Empty,
    };

    public DuckDbSqlBuilder()
        : this(options: null, format: null, arguments: null) { }

    public DuckDbSqlBuilder(FormattableString value, InterpolatedSqlBuilderOptions? options = null)
        : this(options: options, format: null, arguments: null)
    {
        Options.Parser.ParseAppend(this, value);
        ResetAutoSpacing();
    }

    public DuckDbSqlBuilder(IInterpolatedSql value, InterpolatedSqlBuilderOptions? options = null)
        : this(options: options, format: null, arguments: null)
    {
        Append(value);
        ResetAutoSpacing();
    }

    public DuckDbSqlBuilder(string value, InterpolatedSqlBuilderOptions? options = null)
        : this(options: options, format: null, arguments: null)
    {
        AppendLiteral(value);
        ResetAutoSpacing();
    }

    protected DuckDbSqlBuilder(
        InterpolatedSqlBuilderOptions? options = null,
        StringBuilder? format = null,
        List<InterpolatedSqlParameter>? arguments = null
    )
        : base(options: options ?? DefaultOptions, format: format, arguments: arguments) { }

    public override void AppendArgument(object? argument, int alignment = 0, string? format = null)
    {
        // Reformat enumerable arguments as a series of parameter
        // placeholders e.g. three item list becomes ?, ?, ?
        if (argument is not string and IEnumerable enumerable)
        {
            var args = enumerable.Cast<object>().ToArray();
            var index = 0;

            foreach (var arg in args)
            {
                base.AppendArgument(arg, alignment, format);

                if (index < args.Length - 1)
                {
                    base.AppendLiteral(", ");
                }

                index += 1;
            }
        }
        else
        {
            base.AppendArgument(argument, alignment, format);
        }
    }

    public override IInterpolatedSql Build() => base.AsSql();
}
