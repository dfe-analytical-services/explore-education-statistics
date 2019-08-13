using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
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

        public override TableBuilderResultViewModel Query(ObservationQueryContext queryContext)
        {
            var observations = GetObservations(queryContext).AsQueryable();
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