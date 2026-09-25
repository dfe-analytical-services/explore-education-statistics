#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class TimePeriodQueryUtils
{
    public static async Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListDistinctTimePeriods(
        IQueryable<Observation> observations,
        CancellationToken cancellationToken = default
    )
    {
        var timePeriods = (
            await observations.Select(o => new { o.Year, o.TimeIdentifier }).Distinct().ToListAsync(cancellationToken)
        ).Select(tuple => (tuple.Year, tuple.TimeIdentifier));

        return Order(timePeriods);
    }

    public static List<(int Year, TimeIdentifier TimeIdentifier)> Order(
        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> timePeriods
    )
    {
        // Ordering of time periods must be evaluated in memory rather than being translated to a database query.
        // They are expected to be ordered by their definition order, not by their enum value
        return timePeriods.OrderBy(tuple => tuple.Year).ThenBy(tuple => tuple.TimeIdentifier).ToList();
    }
}
