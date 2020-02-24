using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
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

        public override Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            ObservationQueryContext queryContext, ReleaseId? releaseId = null)
        {
            var observations = GetObservations(queryContext, releaseId).AsQueryable();
            if (!observations.Any())
            {
                return Task.FromResult(
                    new Either<ActionResult, TableBuilderResultViewModel>(new TableBuilderResultViewModel()));
            }

            return _resultSubjectMetaService
                .GetSubjectMeta(SubjectMetaQueryContext.FromObservationQueryContext(queryContext), observations)
                .OnSuccess(subjectMetaViewModel =>
                {
                    return new TableBuilderResultViewModel
                    {
                        SubjectMeta = subjectMetaViewModel,
                        Results = observations.Select(observation =>
                            _resultBuilder.BuildResult(observation, queryContext.Indicators))
                    };
                });
        }
    }
}