using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IFilterService _filterService;
        private readonly IIndicatorService _indicatorService;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;

        public MetaGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IFilterService filterService,
            IIndicatorService indicatorService,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _filterService = filterService;
            _indicatorService = indicatorService;
            _statisticsDbContext = statisticsDbContext;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanViewRelease(release))
                .OnSuccess(async release => BuildViewModel(release, await GetSubjects(releaseId)));
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> UpdateRelease(Guid releaseId,
            MetaGuidanceUpdateReleaseViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(async release =>
                {
                    _contentDbContext.Update(release);
                    release.MetaGuidance = request.Content;
                    await _contentDbContext.SaveChangesAsync();

                    return BuildViewModel(release, await GetSubjects(releaseId));
                });
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> UpdateSubject(Guid releaseId,
            Guid subjectId,
            MetaGuidanceUpdateSubjectViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(release =>
                {
                    return _statisticsPersistenceHelper.CheckEntityExists<ReleaseSubject>(
                            releaseSubjects => releaseSubjects
                                .Where(releaseSubject => releaseSubject.ReleaseId == releaseId
                                                         && releaseSubject.SubjectId == subjectId))
                        .OnSuccess(async releaseSubject =>
                        {
                            _statisticsDbContext.Update(releaseSubject);
                            releaseSubject.MetaGuidance = request.Content;
                            await _statisticsDbContext.SaveChangesAsync();

                            return BuildViewModel(release, await GetSubjects(releaseId));
                        });
                });
        }

        private async Task<List<MetaGuidanceSubjectViewModel>> GetSubjects(Guid releaseId)
        {
            var dataFiles = await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId
                             && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data
                             && rf.ReleaseFileReference.SubjectId.HasValue)
                .Select(rf => rf.ReleaseFileReference)
                .ToDictionaryAsync(f => f.SubjectId.Value, f => f);

            var releaseSubjects = await _statisticsDbContext
                .ReleaseSubject
                .Include(s => s.Subject)
                .Where(s => s.ReleaseId == releaseId)
                .ToListAsync();

            return (await Task.WhenAll(
                releaseSubjects
                    .OrderBy(subject => subject.Subject.Name)
                    .Select(async releaseSubject =>
                    {
                        var subject = releaseSubject.Subject;
                        var geographicLevels = await GetGeographicLevels(subject.Id);
                        var timePeriods = await GetTimePeriods(subject.Id);
                        var variables = GetVariables(subject.Id);
                        return new MetaGuidanceSubjectViewModel
                        {
                            Id = subject.Id,
                            Content = releaseSubject.MetaGuidance,
                            Filename = dataFiles[subject.Id].Filename,
                            Name = subject.Name,
                            GeographicLevels = geographicLevels,
                            TimePeriods = timePeriods,
                            Variables = variables
                        };
                    })
            )).ToList();
        }

        private async Task<MetaGuidanceSubjectTimePeriodsViewModel> GetTimePeriods(Guid subjectId)
        {
            var orderedObservations = _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .OrderBy(observation => observation.Year)
                .ThenBy(observation => observation.TimeIdentifier);

            var first = await orderedObservations.FirstOrDefaultAsync();
            var last = await orderedObservations.LastOrDefaultAsync();
            return new MetaGuidanceSubjectTimePeriodsViewModel(first?.GetTimePeriod(), last?.GetTimePeriod());
        }

        private async Task<List<GeographicLevel>> GetGeographicLevels(Guid subjectId)
        {
            return await _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .Select(observation => observation.GeographicLevel)
                .Distinct()
                .ToListAsync();
        }

        private List<LabelValue> GetVariables(Guid subjectId)
        {
            var filters = _filterService.FindMany(filter => filter.SubjectId == subjectId)
                .Select(filter =>
                    new LabelValue(
                        string.IsNullOrWhiteSpace(filter.Hint) ? filter.Label : $"{filter.Label} - {filter.Hint}",
                        filter.Name))
                .ToList();

            var indicators = _indicatorService.GetIndicators(subjectId)
                .Select(indicator => new LabelValue(indicator.Label, indicator.Name));

            return filters.Concat(indicators)
                .OrderBy(labelValue => labelValue.Value)
                .ToList();
        }

        private static MetaGuidanceViewModel BuildViewModel(Release release,
            List<MetaGuidanceSubjectViewModel> subjects)
        {
            return new MetaGuidanceViewModel
            {
                Id = release.Id,
                Content = release.MetaGuidance ?? "",
                Subjects = subjects
            };
        }
    }
}