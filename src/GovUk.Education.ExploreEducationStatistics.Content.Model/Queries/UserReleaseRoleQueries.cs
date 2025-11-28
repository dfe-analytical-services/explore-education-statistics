namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserReleaseRoleQueries
{
    public static IQueryable<UserReleaseRole> WhereForUser(this IQueryable<UserReleaseRole> query, Guid userId) =>
        query.Where(upr => upr.UserId == userId);

    public static IQueryable<UserReleaseRole> WhereForReleaseVersion(
        this IQueryable<UserReleaseRole> query,
        Guid releaseVersionId
    ) => query.Where(upr => upr.ReleaseVersionId == releaseVersionId);

    public static IQueryable<UserReleaseRole> WhereForPublication(
        this IQueryable<UserReleaseRole> query,
        Guid publicationId
    ) => query.Where(upr => upr.ReleaseVersion.Release.PublicationId == publicationId);

    public static IQueryable<UserReleaseRole> WhereRolesIn(
        this IQueryable<UserReleaseRole> query,
        params ReleaseRole[] roles
    ) => query.Where(upr => roles.Contains(upr.Role));
}
