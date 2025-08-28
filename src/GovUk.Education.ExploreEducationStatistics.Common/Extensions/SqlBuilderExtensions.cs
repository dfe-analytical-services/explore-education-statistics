#nullable enable
using InterpolatedSql;
using InterpolatedSql.SqlBuilders;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class SqlBuilderExtensions
{
    /// <summary>
    /// Append multiple literal fragments to the SQL builder.
    /// </summary>
    /// <param name="builder">The SQL builder</param>
    /// <param name="literals">The literal fragments to append</param>
    /// <param name="joinString">A string to join the fragments with</param>
    /// <returns>The builder</returns>
    public static TBuilder AppendRange<TBuilder>(
        this TBuilder builder,
        IEnumerable<string> literals,
        string? joinString = null) where TBuilder : InterpolatedSqlBuilderBase
    {
        return builder.AppendRange(
            items: literals,
            append: builder.AppendLiteral,
            joinString: joinString
        );
    }

    /// <summary>
    /// Append multiple <see cref="FormattableString"/> SQL fragments to the SQL builder.
    /// </summary>
    /// <param name="builder">The SQL builder</param>
    /// <param name="fragments">The SQL fragments to append</param>
    /// <param name="joinString">A string to join the fragments with</param>
    /// <returns></returns>
    public static TBuilder AppendRange<TBuilder>(
        this TBuilder builder,
        IEnumerable<FormattableString> fragments,
        string? joinString = null) where TBuilder : InterpolatedSqlBuilderBase
    {
        return builder.AppendRange(
            items: fragments,
            append: builder.AppendFormattableString,
            joinString: joinString
        );
    }

    /// <summary>
    /// Append multiple SQL builders to the SQL builder.
    /// </summary>
    /// <param name="builder">The SQL builder</param>
    /// <param name="builders">The SQL builders to append</param>
    /// <param name="joinString">A string to join the fragments with</param>
    public static TBuilder AppendRange<TBuilder>(
        this TBuilder builder,
        IEnumerable<IBuildable<IInterpolatedSql>> builders,
        string? joinString = null) where TBuilder : InterpolatedSqlBuilderBase
    {
        return builder.AppendRange(
            items: builders,
            append: b => builder.Append(b.Build()),
            joinString: joinString
        );
    }

    /// <summary>
    /// Append multiple <see cref="IInterpolatedSql"/> fragments to the SQL builder.
    /// </summary>
    /// <param name="builder">The SQL builder</param>
    /// <param name="fragments">The SQL fragments to append</param>
    /// <param name="joinString">A string to join the fragments with</param>
    public static TBuilder AppendRange<TBuilder>(
        this TBuilder builder,
        IEnumerable<IInterpolatedSql> fragments,
        string? joinString = null) where TBuilder : InterpolatedSqlBuilderBase
    {
        return builder.AppendRange(
            items: fragments,
            append: builder.Append,
            joinString: joinString
        );
    }

    private static TBuilder AppendRange<TBuilder, TItem>(
        this TBuilder builder,
        IEnumerable<TItem> items,
        Action<TItem> append,
        string? joinString) where TBuilder : InterpolatedSqlBuilderBase
    {
        var itemsList = items.ToList();

        foreach (var (item, index) in itemsList.WithIndex())
        {
            append(item);

            if (!itemsList.IsLastIndex(index) && joinString is not null)
            {
                builder.AppendLiteral(joinString);
            }
        }

        return builder;
    }
}
