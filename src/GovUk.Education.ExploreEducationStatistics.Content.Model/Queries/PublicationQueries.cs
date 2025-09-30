namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class PublicationQueries
{
    public static IQueryable<Publication> WhereHasPublishedRelease(this IQueryable<Publication> query) =>
        query.Where(p => p.LatestPublishedReleaseVersionId.HasValue);
}
