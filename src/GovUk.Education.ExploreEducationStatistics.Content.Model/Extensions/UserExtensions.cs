#nullable enable
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class UserExtensions
{
    private static Expression<Func<User, bool>> IsPendingInviteExpression =>
        u => !u.Active &&
        !u.SoftDeleted.HasValue
        && u.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays);

    private static Expression<Func<User, bool>> InviteHasExpiredExpression =>
        u => !u.Active &&
        !u.SoftDeleted.HasValue
        && u.Created < DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays);

    public static bool IsPendingInvite(this User user) =>
        IsPendingInviteExpression.Compile()(user);

    public static IQueryable<User> WhereIsPendingInvite(this IQueryable<User> query) =>
        query.Where(IsPendingInviteExpression);

    public static bool InviteHasExpired(this User user) =>
        InviteHasExpiredExpression.Compile()(user);
}
