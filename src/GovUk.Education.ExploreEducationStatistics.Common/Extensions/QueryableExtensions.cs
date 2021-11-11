#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static Task<T[]> ToArrayAsync<T>(
            this IQueryable<T> source,
            CancellationToken? cancellationToken)
        {
            if (cancellationToken != null)
            {
                return source.ToArrayAsync(cancellationToken.Value);
            }

            return source.ToArrayAsync();
        }
    }
}
