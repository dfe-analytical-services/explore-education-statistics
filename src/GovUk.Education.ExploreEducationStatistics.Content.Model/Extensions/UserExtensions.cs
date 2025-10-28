#nullable enable
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class UserExtensions
{
    private static Expression<Func<User, bool>> IsInvitePendingExpression =>
        u =>
            !u.Active
            && !u.SoftDeleted.HasValue
            && u.Created >= DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays);

    private static Expression<Func<User, bool>> IsInviteExpiredExpression =>
        u =>
            !u.Active
            && !u.SoftDeleted.HasValue
            && u.Created < DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays);

    public static bool IsInvitePending(this User user) => IsInvitePendingExpression.Compile()(user);

    public static IQueryable<User> WhereInvitePending(this IQueryable<User> query) =>
        query.Where(IsInvitePendingExpression);

    public static bool IsInviteExpired(this User user) => IsInviteExpiredExpression.Compile()(user);
}
