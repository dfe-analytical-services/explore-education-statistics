#nullable enable
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class UserQueries
{
    private static Expression<Func<User, bool>> IsPendingInviteExpression =>
        u => !u.Active && !u.SoftDeleted.HasValue;

    public static bool IsPendingInvite(this User user) =>
        IsPendingInviteExpression.Compile()(user);

    public static IQueryable<User> WhereIsPendingInvite(this IQueryable<User> query) =>
        query.Where(IsPendingInviteExpression);
}
