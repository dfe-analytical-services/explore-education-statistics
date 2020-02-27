using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderDataService : AbstractDataService<TableBuilderResultViewModel>
    {
        private readonly ITableBuilderResultSubjectMetaService _resultSubjectMetaService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly ISubjectService _subjectService;

        public TableBuilderDataService(IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITableBuilderResultSubjectMetaService resultSubjectMetaService,
            ISubjectService subjectService,
            IUserService userService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder) :
            base(observationService, persistenceHelper, userService)
        {
            _resultBuilder = resultBuilder;
            _resultSubjectMetaService = resultSubjectMetaService;
            _subjectService = subjectService;
        }

        public override Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            ObservationQueryContext queryContext)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(queryContext.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(_ =>
                {
                    var observations = GetObservations(queryContext).AsQueryable();
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
                });
        }

        private async Task<Either<ActionResult, bool>> CheckCanViewSubjectData(Subject subject)
        {
            if (_subjectService.IsSubjectForLatestPublishedRelease(subject.Id) ||
                await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return true;
            }

            return new ForbidResult();
        }
    }
}