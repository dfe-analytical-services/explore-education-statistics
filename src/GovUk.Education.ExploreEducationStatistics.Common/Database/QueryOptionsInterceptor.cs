using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

public class QueryOptionsInterceptor(
    IQueryOptionsInterceptorSqlProcessor sqlProcessor) : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        TryInjectOptions(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        TryInjectOptions(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        TryInjectOptions(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        TryInjectOptions(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        TryInjectOptions(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        TryInjectOptions(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void TryInjectOptions(DbCommand command)
    {
        command.CommandText = sqlProcessor.Process(command.CommandText);
    }
}

public interface IQueryOptionsInterceptorSqlProcessor
{
    string Process(string sql);
}

public partial class QueryOptionsInterceptorSqlProcessor : IQueryOptionsInterceptorSqlProcessor
{
    /// <summary>
    /// A Regex for locating any line or block comments at the start of the SQL statement.
    /// This is where any "WithOptions: OPTION(...)" strings would be expected to be found.
    /// </summary>
    [GeneratedRegex(@"^(?:\s*(?:--[^\n]*\n|/\*.*?\*/\s*))+",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex LeadingCommentsRegexCompile();

    private static readonly Regex LeadingCommentsRegex =
        LeadingCommentsRegexCompile();

    /// <summary>
    /// A Regex for locating special "WithOptions: OPTION(...)" text within SQL statement comments sections.
    /// If this text exists in a query, this interceptor will take action. Otherwise, the query is left alone.
    /// </summary>
    [GeneratedRegex(
        @"WithOptions\s*:\s*OPTION\s*\(\s*(?<Options>(?:[^()]+|\((?<open>)|\)(?<-open>))*)(?(open)(?!))\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex WithOptionsMarkerRegexCompile();

    // Pulls out everything after a "WithOptions: " marker inside the leading comments
    private static readonly Regex WithOptionsMarkerRegex =
        WithOptionsMarkerRegexCompile();

    /// <summary>
    /// A Regex for locating existing OPTION(...) statements in the existing query. If any are found, they
    /// will be merged with any from the "WithOptions: OPTION(...)" string if it exists.
    /// </summary>
    [GeneratedRegex(@"\bOPTION\s*\(\s*(?<opts>(?:[^()]+|\((?<open>)|\)(?<-open>))*)(?(open)(?!))\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OptionClauseRegexCompile();

    // Matches an existing OPTION(...) clause so we can merge
    private static readonly Regex OptionClauseRegex =
        OptionClauseRegexCompile();

    public string Process(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return sql;
        }

        // Identify the leading comment block (where tags live)
        var leading = LeadingCommentsRegex.Match(sql);
        if (!leading.Success)
        {
            return sql;
        }

        // Look for WithOptions: ... ONLY inside the leading comments
        var withOptionsMatches = WithOptionsMarkerRegex.Matches(leading.Value);
        if (withOptionsMatches.Count == 0)
        {
            return sql;
        }

        var allItems = withOptionsMatches
            .Select(m => m.Groups["Options"].Value.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        if (allItems.Length == 0)
        {
            return sql;
        }

        // Aggregate all Optioned items from OPTION(...) in tags
        var extractedItems = withOptionsMatches
            .Select(m => m.Groups["Options"].Value) // only inside OPTION(...)
            .SelectMany(h => h.Split(','))
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (extractedItems.Count == 0)
        {
            return sql;
        }

        var injectedOption = $"OPTION({string.Join(", ", extractedItems)})";

        // Search for OPTION(...) after the leading comments
        var bodyStart = leading.Length; // index where real SQL begins
        var bodyOptionMatch = OptionClauseRegex.Match(sql, bodyStart);

        if (bodyOptionMatch.Success)
        {
            var existing = bodyOptionMatch
                .Groups["opts"]
                .Value
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);

            var merged = existing
                .Concat(extractedItems)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var mergedText = $"OPTION({string.Join(", ", merged)})";

            // Replace the OPTION(...) found in the SQL body (not the comment)
            return sql.Substring(0, bodyOptionMatch.Index)
                   + mergedText
                   + sql.Substring(bodyOptionMatch.Index + bodyOptionMatch.Length);
        }

        // Append OPTION(...) at the end of the SQL body
        return sql.EndsWith(";", StringComparison.Ordinal)
            ? sql.Substring(0, sql.Length - 1) + " " + injectedOption + ";"
            : sql + " " + injectedOption;
    }
}
