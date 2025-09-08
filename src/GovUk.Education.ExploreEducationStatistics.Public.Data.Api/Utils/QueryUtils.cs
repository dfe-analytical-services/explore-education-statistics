using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;

public static class QueryUtils
{
    public static string Path(params string[] segments) =>
        segments.Where(s => !s.IsNullOrWhitespace()).JoinToString('.');
}
