using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderDataService : AbstractDataService<ResultViewModel>
    {
        private readonly IFootnoteService _footnoteService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;

        public TableBuilderDataService(IFootnoteService footnoteService,
            IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder) : base(observationService, subjectService)
        {
            _footnoteService = footnoteService;
            _resultBuilder = resultBuilder;
        }

        public override ResultViewModel Query(ObservationQueryContext queryContext)
        {
            var observations = GetObservations(queryContext).ToList();
            if (!observations.Any())
            {
                return new ResultViewModel();
            }

            return new ResultViewModel
            {
                Footnotes = GetFootnotes(queryContext),
                TimePeriodRange = GetTimePeriodRange(observations),
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
            };
        }

        private IEnumerable<FootnoteViewModel> GetFootnotes(ObservationQueryContext queryContext)
        {
            return _footnoteService.GetFootnotes(queryContext.SubjectId, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Indicators = new List<int>(),
                    Value = footnote.Content
                });
        }

        private static IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(
            IEnumerable<Observation> observations)
        {
            var timePeriods = GetDistinctObservationTimePeriods(observations);

            var start = timePeriods.First();
            var end = timePeriods.Last();

            if (start.TimeIdentifier.IsNumberOfTerms() || end.TimeIdentifier.IsNumberOfTerms())
            {
                return MergeTimePeriodsWithHalfTermRange(timePeriods, start.Year, end.Year)
                    .Select(BuildTimePeriodViewModel);
            }

            return GetTimePeriodRange(start, end).Select(BuildTimePeriodViewModel);
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(
            (int Year, TimeIdentifier TimeIdentifier) start,
            (int Year, TimeIdentifier TimeIdentifier) end)
        {
            return TimePeriodUtil.Range(start.Year, start.TimeIdentifier, end.Year, end.TimeIdentifier);
        }

        private static IEnumerable<(int Year, TimeIdentifier TimeIdentifier)>
            MergeTimePeriodsWithHalfTermRange(
                List<(int Year, TimeIdentifier TimeIdentifier)> timePeriods, int startYear, int endYear)
        {
            // Generate a year range based only on Six Half Terms
            var range = TimePeriodUtil.Range(startYear, SixHalfTerms, endYear, SixHalfTerms);

            // Merge it with the distinct time periods to replace any years which should be Five Half Terms
            var rangeMap = range.ToDictionary(tuple => tuple.Year, tuple => tuple);
            timePeriods.ForEach(tuple => { rangeMap[tuple.Year] = (tuple.Year, tuple.TimeIdentifier); });

            return rangeMap.Values;
        }

        private static List<(int Year, TimeIdentifier TimeIdentifier)> GetDistinctObservationTimePeriods(
            IEnumerable<Observation> observations)
        {
            return observations.Select(o => (o.Year, o.TimeIdentifier))
                .Distinct()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .ToList();
        }

        private static TimePeriodMetaViewModel BuildTimePeriodViewModel((int Year, TimeIdentifier TimeIdentifier) tuple)
        {
            return new TimePeriodMetaViewModel
            {
                Code = tuple.TimeIdentifier,
                Label = TimePeriodLabelFormatter.Format(tuple.Year, tuple.TimeIdentifier),
                Year = tuple.Year
            };
        }
    }
}