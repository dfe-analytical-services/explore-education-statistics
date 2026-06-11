namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserPreReleaseRoleQueries
{
    public static IQueryable<UserPreReleaseRole> WhereForUser(this IQueryable<UserPreReleaseRole> query, Guid userId) =>
        query.Where(urr => urr.UserId == userId);

    public static IQueryable<UserPreReleaseRole> WhereForReleaseVersion(
        this IQueryable<UserPreReleaseRole> query,
        Guid releaseVersionId
    ) => query.Where(urr => urr.ReleaseVersionId == releaseVersionId);

    public static IQueryable<UserPreReleaseRole> WhereForPublication(
        this IQueryable<UserPreReleaseRole> query,
        Guid publicationId
    ) => query.Where(urr => urr.ReleaseVersion.Release.PublicationId == publicationId);

    public static IQueryable<UserPreReleaseRole> WhereEmailNotSent(this IQueryable<UserPreReleaseRole> query) =>
        query.Where(urr => urr.EmailSent == null);

    public static IQueryable<UserPreReleaseRole> WhereUserIsActive(this IQueryable<UserPreReleaseRole> query) =>
        query.Where(urr => urr.User.Active);

    public static IQueryable<UserPreReleaseRole> WhereUserHasPendingInvite(this IQueryable<UserPreReleaseRole> query) =>
        query
            .Where(urr => !urr.User.Active)
            .Where(urr => !urr.User.SoftDeleted.HasValue)
            .Where(urr => urr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays));

    public static IQueryable<UserPreReleaseRole> WhereUserIsActiveOrHasPendingInvite(
        this IQueryable<UserPreReleaseRole> query
    ) =>
        query.Where(urr =>
            urr.User.Active
            || (
                !urr.User.Active
                && !urr.User.SoftDeleted.HasValue
                && urr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays)
            )
        );
}
