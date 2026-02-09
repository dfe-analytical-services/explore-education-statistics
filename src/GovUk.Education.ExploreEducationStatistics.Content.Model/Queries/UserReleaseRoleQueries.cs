using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserReleaseRoleQueries
{
    public static IQueryable<UserReleaseRole> WhereForUser(this IQueryable<UserReleaseRole> query, Guid userId) =>
        query.Where(urr => urr.UserId == userId);

    public static IQueryable<UserReleaseRole> WhereForReleaseVersion(
        this IQueryable<UserReleaseRole> query,
        Guid releaseVersionId
    ) => query.Where(urr => urr.ReleaseVersionId == releaseVersionId);

    public static IQueryable<UserReleaseRole> WhereForPublication(
        this IQueryable<UserReleaseRole> query,
        Guid publicationId
    ) => query.Where(urr => urr.ReleaseVersion.Release.PublicationId == publicationId);

    public static IQueryable<UserReleaseRole> WhereRolesIn(
        this IQueryable<UserReleaseRole> query,
        params ReleaseRole[] roles
    )
    {
        if (roles.IsNullOrEmpty())
        {
            throw new ArgumentException($"{nameof(roles)} should not be empty or NULL.");
        }

        return query.Where(urr => roles.Contains(urr.Role));
    }

    public static IQueryable<UserReleaseRole> WhereRolesNotIn(
        this IQueryable<UserReleaseRole> query,
        params ReleaseRole[] roles
    )
    {
        if (roles.IsNullOrEmpty())
        {
            throw new ArgumentException($"{nameof(roles)} should not be empty or NULL.");
        }

        return query.Where(urr => !roles.Contains(urr.Role));
    }

    public static IQueryable<UserReleaseRole> WhereEmailNotSent(this IQueryable<UserReleaseRole> query) =>
        query.Where(urr => urr.EmailSent == null);

    public static IQueryable<UserReleaseRole> WhereUserIsActive(this IQueryable<UserReleaseRole> query) =>
        query.Where(urr => urr.User.Active);

    public static IQueryable<UserReleaseRole> WhereUserHasPendingInvite(this IQueryable<UserReleaseRole> query) =>
        query
            .Where(urr => !urr.User.Active)
            .Where(urr => !urr.User.SoftDeleted.HasValue)
            .Where(urr => urr.User.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays));

    public static IQueryable<UserReleaseRole> WhereUserIsActiveOrHasPendingInvite(
        this IQueryable<UserReleaseRole> query
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
