using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class MetaGuidanceSubjectService : IMetaGuidanceSubjectService
    {
        private readonly IFilterService _filterService;
        private readonly IIndicatorService _indicatorService;
        private readonly StatisticsDbContext _context;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;

        public MetaGuidanceSubjectService(IFilterService filterService,
            IIndicatorService indicatorService,
            StatisticsDbContext context,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper)
        {
            _filterService = filterService;
            _indicatorService = indicatorService;
            _context = context;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, List<MetaGuidanceSubjectViewModel>>> GetSubjects(Guid releaseId,
            List<Guid> subjectIds = null)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release =>
                {
                    var releaseSubjectsQueryable = _context
                        .ReleaseSubject
                        .Include(s => s.Subject)
                        .Where(rs => rs.ReleaseId == releaseId);

                    if (subjectIds != null)
                    {
                        releaseSubjectsQueryable =
                            releaseSubjectsQueryable.Where(rs => subjectIds.Contains(rs.SubjectId));
                    }

                    var releaseSubjects = await releaseSubjectsQueryable
                        .OrderBy(rs => rs.Subject.Name)
                        .ToListAsync();

                    var result = new List<MetaGuidanceSubjectViewModel>();
                    await releaseSubjects.ForEachAsync(async releaseSubject =>
                    {
                        result.Add(await BuildSubjectViewModel(releaseSubject));
                    });

                    return result;
                })
                // Currently we expect a failure checking the Release exists and succeed with an empty list.
                // StatisticsDb Releases are not always in sync with ContentDb Releases.
                // Until the first Subject is imported, no StatisticsDb Release exists.
                .OnFailureSucceedWith(result => Task.FromResult(new List<MetaGuidanceSubjectViewModel>()));
        }

        public async Task<Either<ActionResult, bool>> Validate(Guid releaseId)
        {
            var releaseSubjects = _context
                .ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId);

            return !await releaseSubjects.AnyAsync() || !await releaseSubjects.AnyAsync(
                rs => string.IsNullOrWhiteSpace(rs.MetaGuidance));
        }

        private async Task<MetaGuidanceSubjectTimePeriodsViewModel> GetTimePeriods(Guid subjectId)
        {
            var orderedTimePeriods = _context
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
                TimePeriodLabelFormatter.Format(last.Year, last.TimeIdentifier));
        }

        private async Task<List<string>> GetGeographicLevels(Guid subjectId)
        {
            return await _context
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

        private async Task<MetaGuidanceSubjectViewModel> BuildSubjectViewModel(ReleaseSubject releaseSubject)
        {
            var subject = releaseSubject.Subject;
            var geographicLevels = await GetGeographicLevels(subject.Id);
            var timePeriods = await GetTimePeriods(subject.Id);
            var variables = GetVariables(subject.Id);
            return new MetaGuidanceSubjectViewModel
            {
                Id = subject.Id,
                Content = releaseSubject.MetaGuidance ?? "",
                Filename = subject.Filename ?? "Unknown",
                Name = subject.Name,
                GeographicLevels = geographicLevels,
                TimePeriods = timePeriods,
                Variables = variables
            };
        }
    }
}