namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserPreReleaseRoleQueries
{
    public static IQueryable<UserPreReleaseRole> WhereForUser(this IQueryable<UserPreReleaseRole> query, Guid userId) =>
        query.Where(uprr => uprr.UserId == userId);

    public static IQueryable<UserPreReleaseRole> WhereForReleaseVersion(
        this IQueryable<UserPreReleaseRole> query,
        Guid releaseVersionId
    ) => query.Where(uprr => uprr.ReleaseVersionId == releaseVersionId);

    public static IQueryable<UserPreReleaseRole> WhereForPublication(
        this IQueryable<UserPreReleaseRole> query,
        Guid publicationId
    ) => query.Where(uprr => uprr.ReleaseVersion.Release.PublicationId == publicationId);

    public static IQueryable<UserPreReleaseRole> WhereEmailNotSent(this IQueryable<UserPreReleaseRole> query) =>
        query.Where(uprr => uprr.EmailSent == null);

    public static IQueryable<UserPreReleaseRole> WhereUserIsActive(this IQueryable<UserPreReleaseRole> query) =>
        query.Where(uprr => uprr.User.Active);

    public static IQueryable<UserPreReleaseRole> WhereUserHasPendingInvite(this IQueryable<UserPreReleaseRole> query) =>
        query
            .Where(uprr => !uprr.User.Active)
            .Where(uprr => !uprr.User.SoftDeleted.HasValue)
            .Where(uprr => uprr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays));

    public static IQueryable<UserPreReleaseRole> WhereUserIsActiveOrHasPendingInvite(
        this IQueryable<UserPreReleaseRole> query
    ) =>
        query.Where(uprr =>
            uprr.User.Active
            || (
                !uprr.User.Active
                && !uprr.User.SoftDeleted.HasValue
                && uprr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays)
            )
        );
}
