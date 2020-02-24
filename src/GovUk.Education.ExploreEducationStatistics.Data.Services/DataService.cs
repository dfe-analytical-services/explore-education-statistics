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

        public override Task<Either<ActionResult, ResultWithMetaViewModel>> Query(
            ObservationQueryContext queryContext, ReleaseId? releaseId = null)
        {
            var observations = GetObservations(queryContext, releaseId).AsQueryable();

            if (!observations.Any())
            {
                return Task.FromResult(new Either<ActionResult, ResultWithMetaViewModel>(new ResultWithMetaViewModel()));
            }

            var result = observations
                .Select(observation => _resultBuilder.BuildResult(observation, queryContext.Indicators))
                .ToList();

            return _subjectMetaService.GetSubjectMeta(SubjectMetaQueryContext.FromObservationQueryContext(queryContext),
                    observations).OnSuccess(subjectMetaViewModel => new ResultWithMetaViewModel
                {
                    MetaData = subjectMetaViewModel,
                    Result = result
                });
        }
    }
}