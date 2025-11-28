namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserPublicationRoleQueries
{
    public static IQueryable<UserPublicationRole> WhereForUser(
        this IQueryable<UserPublicationRole> query,
        Guid userId
    ) => query.Where(upr => upr.UserId == userId);

    public static IQueryable<UserPublicationRole> WhereForPublication(
        this IQueryable<UserPublicationRole> query,
        Guid publicationId
    ) => query.Where(upr => upr.PublicationId == publicationId);

    public static IQueryable<UserPublicationRole> WhereRolesIn(
        this IQueryable<UserPublicationRole> query,
        params PublicationRole[] roles
    ) => query.Where(upr => roles.Contains(upr.Role));
}
