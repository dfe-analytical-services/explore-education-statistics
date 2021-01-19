using System;
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
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IResultSubjectMetaService _resultSubjectMetaService;
        private readonly ISubjectService _subjectService;
        private readonly IUserService _userService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly IReleaseService _releaseService;

        public TableBuilderService(
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            IResultSubjectMetaService resultSubjectMetaService,
            ISubjectService subjectService,
            IUserService userService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            IReleaseService releaseService)
        {
            _observationService = observationService;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _resultSubjectMetaService = resultSubjectMetaService;
            _subjectService = subjectService;
            _userService = userService;
            _resultBuilder = resultBuilder;
            _releaseService = releaseService;
        }

        public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(ObservationQueryContext queryContext)
        {
            var publication = await _subjectService.GetPublicationForSubject(queryContext.SubjectId);
            var release = _releaseService.GetLatestPublishedRelease(publication.Id);

            if (release == null)
            {
                return new NotFoundResult();
            }

            return await Query(release, queryContext);
        }

        public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            Guid releaseId,
            ObservationQueryContext queryContext)
        {
            return await _statisticsPersistenceHelper.CheckEntityExists<ReleaseSubject>(
                    query => query
                        .Include(rs => rs.Release)
                        .Where(rs => rs.ReleaseId == releaseId
                                     && rs.SubjectId == queryContext.SubjectId)
                )
                .OnSuccess(rs => Query(rs.Release, queryContext));
        }

        private async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            Release release,
            ObservationQueryContext queryContext)
        {
            return await _statisticsPersistenceHelper.CheckEntityExists<Subject>(queryContext.SubjectId)
                .OnSuccessDo(CheckCanViewSubjectData)
                .OnSuccess(async () =>
                {
                    var observations = _observationService.FindObservations(queryContext).AsQueryable();

                    if (!observations.Any())
                    {
                        return new TableBuilderResultViewModel();
                    }

                    return await _resultSubjectMetaService
                        .GetSubjectMeta(release.Id, SubjectMetaQueryContext.FromObservationQueryContext(queryContext), observations)
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
            if (await _subjectService.IsSubjectForLatestPublishedRelease(subject.Id) ||
                await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return subject;
            }

            return new ForbidResult();
        }
    }
}