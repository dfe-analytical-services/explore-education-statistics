#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class TimePeriodService : ITimePeriodService
{
    private readonly StatisticsDbContext _context;

    public TimePeriodService(StatisticsDbContext context)
    {
        _context = context;
    }

    public Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetTimePeriods(Guid subjectId)
    {
        var observationsQuery = _context
            .Observation.AsNoTracking()
            .Where(observation => observation.SubjectId == subjectId);

        return GetDistinctObservationTimePeriods(observationsQuery);
    }

    public Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetTimePeriods(
        IQueryable<Observation> observationsQuery
    )
    {
        return GetDistinctObservationTimePeriods(observationsQuery);
    }

    public IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(IList<Observation> observations)
    {
        var timePeriods = GetDistinctObservationTimePeriods(observations);

        var start = timePeriods.First();
        var end = timePeriods.Last();

        var range = TimePeriodUtil.GetTimePeriodRange(start, end);

        if (!start.TimeIdentifier.IsTerm() || !end.TimeIdentifier.IsTerm())
        {
            return range;
        }

        // For a range of academic terms, only return the terms that are present in the observation data.
        // This avoids warning users in the 'Explore data' step of Table Tool that
        // 'Some rows and columns are not shown in this table as the data does not exist in the underlying file'
        // when the full range of term identifiers is not expected to have data.
        //
        // For example, given a range from '2025/26 Autumn term' to '2025/26 Summer term', the generated range is:
        // '2025/26 Autumn term', '2025/26 Spring term', '2025/26 Autumn and spring term', and '2025/26 Summer term'.
        //
        // If observation data only exists for '2025/26 Autumn term' and '2025/26 Summer term', excluding the missing
        // intermediate terms prevents the unnecessary warning.
        var timePeriodsSet = timePeriods.ToHashSet();
        return range.Where(timePeriodsSet.Contains).ToList();
    }

    public async Task<TimePeriodLabels> GetTimePeriodLabels(Guid subjectId)
    {
        var orderedTimePeriods = await GetTimePeriods(subjectId);

        if (!orderedTimePeriods.Any())
        {
            return new TimePeriodLabels();
        }

        var first = orderedTimePeriods.First();
        var last = orderedTimePeriods.Last();

        return new TimePeriodLabels(
            TimePeriodLabelFormatter.Format(first.Year, first.TimeIdentifier),
            TimePeriodLabelFormatter.Format(last.Year, last.TimeIdentifier)
        );
    }

    private static async Task<IList<(int Year, TimeIdentifier TimeIdentifier)>> GetDistinctObservationTimePeriods(
        IQueryable<Observation> observationsQuery
    )
    {
        var timePeriods = (
            await observationsQuery.Select(o => new { o.Year, o.TimeIdentifier }).Distinct().ToListAsync()
        ).Select(tuple => (tuple.Year, tuple.TimeIdentifier));

        return OrderTimePeriods(timePeriods);
    }

    private static IList<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
        IList<Observation> observations
    )
    {
        var timePeriods = observations.Select(o => (o.Year, o.TimeIdentifier)).Distinct();

        return OrderTimePeriods(timePeriods);
    }

    private static List<(int Year, TimeIdentifier TimeIdentifier)> OrderTimePeriods(
        IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> timePeriods
    )
    {
        // Ordering of time periods must be evaluated in memory rather than being translated to a database query.
        // They are expected to be ordered by their definition order, not by their enum value
        return timePeriods.OrderBy(tuple => tuple.Year).ThenBy(tuple => tuple.TimeIdentifier).ToList();
    }
}
