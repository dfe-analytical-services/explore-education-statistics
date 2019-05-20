using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class CombinedService : ICombinedService
    {
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;
        private readonly IMetaService _metaService;
        private readonly IGeoJsonService _geoJsonService;

        private readonly IResultBuilder<Observation, TableBuilderObservationViewModel> _resultBuilder;

        public CombinedService(
            IGeoJsonService geoJsonService,
            IObservationService observationService,
            ISubjectService subjectService,
            IMetaService metaService,
                IResultBuilder<Observation, TableBuilderObservationViewModel> resultBuilder)
        {
            _geoJsonService = geoJsonService;
            _observationService = observationService;
            _subjectService = subjectService;
            _metaService = metaService;
            _resultBuilder = resultBuilder;
        }

        public ResultViewModel Query(ObservationQueryContext queryContext)
        {
            var observations = GetObservations(queryContext).ToList();
            if (!observations.Any())
            {
                return new ResultViewModel();
            }

            var first = observations.FirstOrDefault();
            
            var geoJson = _geoJsonService.Find("E92000001");

            return new ResultViewModel
            {
                PublicationId = first.Subject.Release.PublicationId,
                ReleaseId = first.Subject.Release.Id,
                SubjectId = first.Subject.Id,
                ReleaseDate = first.Subject.Release.ReleaseDate,
                GeographicLevel = first.GeographicLevel,
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators)),
                MetaData = _metaService.GetSubjectMeta(queryContext.ToSubjectMetaQueryContext())
            };
        }

        private IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext)
        {
            if (!_subjectService.IsSubjectForLatestRelease(queryContext.SubjectId))
            {
                // TODO throw exception?
                return new List<Observation>();
            }

            return _observationService.FindObservations(queryContext);
        }
    }
}