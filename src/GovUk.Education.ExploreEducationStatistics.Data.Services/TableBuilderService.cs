using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly IResultSubjectMetaService _resultSubjectMetaService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ISubjectService _subjectService;
        private readonly IUserService _userService;

        public TableBuilderService(IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            IResultSubjectMetaService resultSubjectMetaService,
            ISubjectService subjectService,
            IUserService userService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder)
        {
            _observationService = observationService;
            _resultBuilder = resultBuilder;
            _resultSubjectMetaService = resultSubjectMetaService;
            _persistenceHelper = persistenceHelper;
            _subjectService = subjectService;
            _userService = userService;
        }

        public Task<Either<ActionResult, TableBuilderResultViewModel>> Query(ObservationQueryContext queryContext)
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

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (_subjectService.IsSubjectForLatestPublishedRelease(subject.Id) ||
                await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return subject;
            }

            return new ForbidResult();
        }

        private IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext)
        {
            return _observationService.FindObservations(queryContext);
        }
    }
}