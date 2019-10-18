using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderDataService : AbstractDataService<TableBuilderResultViewModel>
    {
        private readonly ITableBuilderResultSubjectMetaService _resultSubjectMetaService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;

        public TableBuilderDataService(IObservationService observationService,
            ITableBuilderResultSubjectMetaService resultSubjectMetaService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder) : base(observationService, subjectService)
        {
            _resultBuilder = resultBuilder;
            _resultSubjectMetaService = resultSubjectMetaService;
        }

        public override TableBuilderResultViewModel Query(ObservationQueryContext queryContext, ReleaseId? releaseId = null)
        {
            var observations = GetObservations(queryContext, releaseId).AsQueryable();
            if (!observations.Any())
            {
                return new TableBuilderResultViewModel();
            }

            var subjectMetaViewModel =
                _resultSubjectMetaService.GetSubjectMeta(queryContext.ToSubjectMetaQueryContext(), observations);

            return new TableBuilderResultViewModel
            {
                SubjectMeta = subjectMetaViewModel,
                Results = observations.Select(observation =>
                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
            };
        }
    }
}