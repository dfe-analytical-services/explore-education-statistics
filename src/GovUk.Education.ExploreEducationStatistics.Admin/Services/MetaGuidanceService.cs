using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
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
        private readonly IUserService _userService;

        public MetaGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IFilterService filterService,
            IIndicatorService indicatorService,
            StatisticsDbContext statisticsDbContext,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _filterService = filterService;
            _indicatorService = indicatorService;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanViewRelease(release))
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Update(
            Guid releaseId,
            MetaGuidanceUpdateViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(async release =>
                {
                    _contentDbContext.Update(release);
                    release.MetaGuidance = request.Content;
                    await _contentDbContext.SaveChangesAsync();

                    await UpdateSubjects(releaseId, request.Subjects);

                    return await BuildViewModel(release);
                });
        }

        private async Task UpdateSubjects(
            Guid releaseId,
            List<MetaGuidanceUpdateSubjectViewModel> subjects)
        {
            if (!subjects.Any())
            {
                return;
            }

            var contentById = subjects.ToDictionary(
                s => s.Id,
                s => s.Content
            );
            var subjectIds = subjects.Select(s => s.Id);

            var matchingSubjects = await _statisticsDbContext.ReleaseSubject
                .Where(
                    releaseSubject => releaseSubject.ReleaseId == releaseId
                                      && subjectIds.Contains(releaseSubject.SubjectId)
                )
                .ToListAsync();

            matchingSubjects.ForEach(
                releaseSubject =>
                {
                    var content = contentById.GetValueOrDefault(releaseSubject.SubjectId);

                    if (string.IsNullOrEmpty(content))
                    {
                        return;
                    }

                    releaseSubject.MetaGuidance = content;
                    _statisticsDbContext.Update(releaseSubject);
                });

            await _statisticsDbContext.SaveChangesAsync();
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

            var result = new List<MetaGuidanceSubjectViewModel>();
            await releaseSubjects.ForEachAsync(async releaseSubject =>
            {
                result.Add(await BuildSubjectViewModel(releaseSubject, dataFiles));
            });

            return result;
        }

        private async Task<MetaGuidanceSubjectTimePeriodsViewModel> GetTimePeriods(Guid subjectId)
        {
            var orderedTimePeriods = _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .Select(observation => new {observation.Year, observation.TimeIdentifier})
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier);

            if (!orderedTimePeriods.Any())
            {
                return new MetaGuidanceSubjectTimePeriodsViewModel();
            }

            var first = await orderedTimePeriods.FirstAsync();
            var last = await orderedTimePeriods.LastAsync();

            return new MetaGuidanceSubjectTimePeriodsViewModel(
                TimePeriodLabelFormatter.Format(first.Year, first.TimeIdentifier),
                TimePeriodLabelFormatter.Format(last.Year, last.TimeIdentifier)
            );
        }

        private async Task<List<string>> GetGeographicLevels(Guid subjectId)
        {
            return await _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .Select(observation => observation.GeographicLevel.GetEnumLabel())
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

        private async Task<MetaGuidanceViewModel> BuildViewModel(Release release)
        {
            var subjects = await GetSubjects(release.Id);
            return new MetaGuidanceViewModel
            {
                Id = release.Id,
                Content = release.MetaGuidance ?? "",
                Subjects = subjects
            };
        }

        private async Task<MetaGuidanceSubjectViewModel> BuildSubjectViewModel(ReleaseSubject releaseSubject,
            IReadOnlyDictionary<Guid, ReleaseFileReference> dataFiles)
        {
            var subject = releaseSubject.Subject;
            var geographicLevels = await GetGeographicLevels(subject.Id);
            var timePeriods = await GetTimePeriods(subject.Id);
            var variables = GetVariables(subject.Id);
            return new MetaGuidanceSubjectViewModel
            {
                Id = subject.Id,
                Content = releaseSubject.MetaGuidance ?? "",
                Filename = dataFiles[subject.Id].Filename,
                Name = subject.Name,
                GeographicLevels = geographicLevels,
                TimePeriods = timePeriods,
                Variables = variables
            };
        }
    }
}