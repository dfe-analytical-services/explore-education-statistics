using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class InterpolatedSqlExtensions
{
    public static bool IsEmpty(this IInterpolatedSql sql) =>
        sql.Sql.IsNullOrWhitespace() && sql.SqlParameters.Count == 0;
}
