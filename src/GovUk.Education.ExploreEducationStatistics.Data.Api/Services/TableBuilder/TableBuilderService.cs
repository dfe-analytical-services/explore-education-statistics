using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;

        private readonly IResultBuilder<Observation, TableBuilderObservationViewModel> _resultBuilder;

        public TableBuilderService(
            IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, TableBuilderObservationViewModel> resultBuilder)
        {
            _observationService = observationService;
            _subjectService = subjectService;
            _resultBuilder = resultBuilder;
        }

        public TableBuilderResultViewModel Query(IQueryContext<Observation> queryContext)
        {
            var observations = GetObservations(queryContext);
            if (!observations.Any())
            {
                return new TableBuilderResultViewModel();
            }

            var first = observations.FirstOrDefault();
            return new TableBuilderResultViewModel
            {
                PublicationId = first.Subject.Release.PublicationId,
                ReleaseId = first.Subject.Release.Id,
                SubjectId = first.Subject.Id,
                ReleaseDate = first.Subject.Release.ReleaseDate,
                GeographicLevel = first.GeographicLevel,
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
            };
        }

        private IEnumerable<Observation> GetObservations(IQueryContext<Observation> queryContext)
        {
            if (!_subjectService.IsSubjectForLatestRelease(queryContext.SubjectId))
            {
                // TODO throw exception?
                return new List<Observation>();
            }

            return _observationService.FindObservations(queryContext.FindExpression(), queryContext.Filters);
        }
    }
}