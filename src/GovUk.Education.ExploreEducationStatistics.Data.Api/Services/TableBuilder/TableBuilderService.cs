using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly IReleaseService _releaseService;
        private readonly IObservationService _observationService;
        private readonly IResultBuilder<Observation, TableBuilderObservation> _resultBuilder;

        public TableBuilderService(
            IReleaseService releaseService,
            IObservationService observationService,
            IResultBuilder<Observation, TableBuilderObservation> resultBuilder)
        {
            _releaseService = releaseService;
            _observationService = observationService;
            _resultBuilder = resultBuilder;
        }

        public TableBuilderResult Query(IQueryContext<Observation> queryContext)
        {
            var observations = GetObservations(queryContext);
            if (!observations.Any())
            {
                return new TableBuilderResult();
            }

            var first = observations.FirstOrDefault();
            return new TableBuilderResult
            {
                PublicationId = first.Subject.Release.PublicationId,
                ReleaseId = first.SubjectId,
                ReleaseDate = first.Subject.Release.ReleaseDate,
                GeographicLevel = first.GeographicLevel,
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
            };
        }

        private IEnumerable<Observation> GetObservations(IQueryContext<Observation> queryContext)
        {
            var releaseId = _releaseService.GetLatestRelease(queryContext.PublicationId);
            return _observationService.FindMany(queryContext.FindExpression(releaseId),
                new List<Expression<Func<Observation, object>>>
                    {data => data.Subject, data => data.Subject.Release}
            );
        }
    }
}