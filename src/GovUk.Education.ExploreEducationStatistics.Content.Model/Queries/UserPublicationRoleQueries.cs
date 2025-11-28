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

    public static IQueryable<UserPublicationRole> WhereRolesNotIn(
        this IQueryable<UserPublicationRole> query,
        params PublicationRole[] roles
    ) => query.Where(upr => !roles.Contains(upr.Role));

    public static IQueryable<UserPublicationRole> WhereEmailNotSent(this IQueryable<UserPublicationRole> query) =>
        query.Where(upr => upr.EmailSent == null);

    public static IQueryable<UserPublicationRole> WhereUserIsActive(this IQueryable<UserPublicationRole> query) =>
        query.Where(upr => upr.User.Active);

    public static IQueryable<UserPublicationRole> WhereUserHasPendingInvite(
        this IQueryable<UserPublicationRole> query
    ) =>
        query
            .Where(upr => !upr.User.Active)
            .Where(upr => !upr.User.SoftDeleted.HasValue)
            .Where(upr => upr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays));
}
