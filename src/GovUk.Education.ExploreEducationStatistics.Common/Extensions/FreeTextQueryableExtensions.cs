#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class FreeTextQueryableExtensions
{
    public static IQueryable<FreeTextValueResult<TSource>> JoinFreeText<TSource>(
        this IQueryable<TSource> queryable,
        Func<string, IQueryable<FreeTextRank>> freeTextRankQueryable,
        Expression<Func<TSource, Guid>> sourceKeySelector,
        string? searchTerm)
    {
        if (searchTerm != null)
        {
            return queryable.Join(freeTextRankQueryable(searchTerm),
                sourceKeySelector,
                freeTextRank => freeTextRank.Id,
                (source, freeTextRank) => new FreeTextValueResult<TSource>
                {
                    Value = source,
                    Rank = freeTextRank.Rank
                });
        }

        // If no search term is provided, change the shape of the queryable to be consistent with the
        // shape of the queryable returned by the above join
        return queryable.Select(source => new FreeTextValueResult<TSource>
        {
            Value = source
        });
    }
}
