using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class DataService : AbstractDataService<ResultWithMetaViewModel>
    {
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly ISubjectMetaService _subjectMetaService;

        public DataService(IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            ISubjectMetaService subjectMetaService,
            IUserService userService) : base(observationService, persistenceHelper, userService)
        {
            _resultBuilder = resultBuilder;
            _subjectMetaService = subjectMetaService;
        }

        public override Task<Either<ActionResult, ResultWithMetaViewModel>> Query(ObservationQueryContext queryContext)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(queryContext.SubjectId, HydrateSubject)
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

                    return _subjectMetaService
                        .GetSubjectMeta(SubjectMetaQueryContext.FromObservationQueryContext(queryContext),
                            observations)
                        .OnSuccess(subjectMetaViewModel => new ResultWithMetaViewModel
                        {
                            MetaData = subjectMetaViewModel,
                            Result = result
                        });
                });
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (await _userService.MatchesPolicy(subject.Release, CanViewSubjectDataForRelease))
            {
                return subject;
            }

            return new ForbidResult();
        }
        
        private static IQueryable<Subject> HydrateSubject(IQueryable<Subject> queryable)
        {
            return queryable.Include(subject => subject.Release);
        }
    }
}