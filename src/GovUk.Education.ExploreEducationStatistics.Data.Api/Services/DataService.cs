using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataService : AbstractDataService<ResultWithMetaViewModel>
    {
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly ISubjectMetaService _subjectMetaService;

        public DataService(IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            ISubjectMetaService subjectMetaService) : base(observationService, subjectService)
        {
            _resultBuilder = resultBuilder;
            _subjectMetaService = subjectMetaService;
        }

        public override ResultWithMetaViewModel Query(ObservationQueryContext queryContext)
        {
            var observations = GetObservations(queryContext).AsQueryable();
            if (!observations.Any())
            {
                return new ResultWithMetaViewModel();
            }

            return new ResultWithMetaViewModel
            {
                Result = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators)),
                MetaData = _subjectMetaService.GetSubjectMeta(queryContext.ToSubjectMetaQueryContext(), observations)
            };
        }
    }
}