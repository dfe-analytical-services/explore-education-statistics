#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class MethodologyQueries
{
    public static IQueryable<Methodology> WhereHasPublishedMethodologyVersion(this IQueryable<Methodology> query) =>
        query.Where(m => m.LatestPublishedVersionId.HasValue);
}
