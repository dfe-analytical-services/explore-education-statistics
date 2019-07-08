using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
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

        private static Dictionary<string, TimePeriodMetaViewModel> GetTimePeriodRange(
            IEnumerable<Observation> observations)
        {
            var timePeriods = observations.Select(o => new {o.TimeIdentifier, o.Year})
                .Distinct()
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier);

            // TODO DFE-886 determine minimum and maximum time period from the observations
            var minYear = 2012;
            var minIdentifier = TimeIdentifier.SixHalfTerms;
            var maxYear = 2018;
            var maxIdentifier = TimeIdentifier.SixHalfTerms;

            var range = TimePeriodUtil.Range(minYear, minIdentifier, maxYear, maxIdentifier);

            // TODO DFE-886 there could be values in the range that the table tool doesn’t want returning
            
            return range.ToDictionary(
                tuple => $"{tuple.Year}_{tuple.TimeIdentifier.GetEnumValue()}",
                tuple => new TimePeriodMetaViewModel
                {
                    Code = tuple.TimeIdentifier,
                    Label = TimePeriodLabelFormatter.Format(tuple.Year, tuple.TimeIdentifier),
                    Year = tuple.Year
                });
        }
    }
}