using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class DataService : AbstractDataService<ResultWithMetaViewModel>
    {
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly ISubjectMetaService _subjectMetaService;

        public DataService(IObservationService observationService,
            ISubjectService subjectService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            ISubjectMetaService subjectMetaService,
            IUserService userService) : base(observationService, subjectService, userService)
        {
            _resultBuilder = resultBuilder;
            _subjectMetaService = subjectMetaService;
        }

        public override Task<Either<ActionResult, ResultWithMetaViewModel>> Query(ObservationQueryContext queryContext)
        {
            return CheckSubjectExists(queryContext.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(_ =>
                {
                    var observations = GetObservations(queryContext).AsQueryable();

                    if (!observations.Any())
                    {
                        return Task.FromResult(
                            new Either<ActionResult, ResultWithMetaViewModel>(new ResultWithMetaViewModel()));
                    }

                    var result = observations
                        .Select(observation => _resultBuilder.BuildResult(observation, queryContext.Indicators))
                        .ToList();

                    return _subjectMetaService.GetSubjectMeta(SubjectMetaQueryContext.FromObservationQueryContext(queryContext), observations)
                        .OnSuccess(subjectMetaViewModel => new ResultWithMetaViewModel
                    {
                        MetaData = subjectMetaViewModel,
                        Result = result
                    });
                });
        }
        
        private async Task<Either<ActionResult, bool>> CheckCanViewSubjectData(Subject subject)
        {
            var result = subject.Release.Live || await _userService.MatchesPolicy(subject, CanViewSubjectData);
            return result ? new Either<ActionResult, bool>(true) : new ForbidResult();
        }
    }
}