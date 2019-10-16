using System.Diagnostics;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.Extensions.Logging;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class DataService : AbstractDataService<ResultWithMetaViewModel>
    {
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly ISubjectMetaService _subjectMetaService;
        private readonly ILogger _logger;

        public DataService(IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            ISubjectMetaService subjectMetaService,
            ILogger<DataService> logger) : base(observationService, subjectService)
        {
            _resultBuilder = resultBuilder;
            _subjectMetaService = subjectMetaService;
            _logger = logger;
        }

        public override ResultWithMetaViewModel Query(ObservationQueryContext queryContext, ReleaseId? releaseId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var observations = GetObservations(queryContext, releaseId).AsQueryable();

            _logger.LogTrace("Got Observations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            if (!observations.Any())
            {
                return new ResultWithMetaViewModel();
            }

            var result = observations
                .Select(observation => _resultBuilder.BuildResult(observation, queryContext.Indicators)).ToList();

            _logger.LogTrace("Built Observation results in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var metaData = _subjectMetaService.GetSubjectMeta(queryContext.ToSubjectMetaQueryContext(), observations);

            _logger.LogTrace("Got meta data for Observations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Stop();

            return new ResultWithMetaViewModel
            {
                Result = result,
                MetaData = metaData
            };
        }
    }
}