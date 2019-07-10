using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderDataService : AbstractDataService<ResultViewModel>
    {
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;

        public TableBuilderDataService(IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder) : base(observationService, subjectService)
        {
            _resultBuilder = resultBuilder;
        }

        public override ResultViewModel Query(ObservationQueryContext queryContext)
        {
            var observations = GetObservations(queryContext).ToList();
            if (!observations.Any())
            {
                return new ResultViewModel();
            }

            var first = observations.FirstOrDefault();
            return new ResultViewModel
            {
                PublicationId = first.Subject.Release.PublicationId,
                ReleaseId = first.Subject.Release.Id,
                SubjectId = first.Subject.Id,
                ReleaseDate = first.Subject.Release.ReleaseDate,
                TimePeriodRange = GetTimePeriodRange(observations),
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
            };
        }

        private static IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(
            IEnumerable<Observation> observations)
        {
            var timePeriods = observations.Select(o => (o.Year, o.TimeIdentifier))
                .Distinct()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
                .ToList();

            var first = timePeriods.First();
            var last = timePeriods.Last();

            var range = TimePeriodUtil.Range(first.Year, first.TimeIdentifier, last.Year, last.TimeIdentifier);

            // TODO DFE-886 there could be values in the range that the table tool doesn’t want returning

            return range.Select(tuple => new TimePeriodMetaViewModel
            {
                Code = tuple.TimeIdentifier,
                Label = TimePeriodLabelFormatter.Format(tuple.Year, tuple.TimeIdentifier),
                Year = tuple.Year
            });
        }
    }
}