using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
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

        public async Task<Either<ActionResult, List<MetaGuidanceSubjectViewModel>>> GetSubjects(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release =>
                {
                    var releaseSubjects = await _context
                        .ReleaseSubject
                        .Include(s => s.Subject)
                        .Where(s => s.ReleaseId == releaseId)
                        .ToListAsync();

                    var result = new List<MetaGuidanceSubjectViewModel>();
                    await releaseSubjects.ForEachAsync(async releaseSubject =>
                    {
                        result.Add(await BuildSubjectViewModel(releaseSubject));
                    });

                    return result;
                });
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
                (first.Year, first.TimeIdentifier).GetTimePeriod(),
                (last.Year, last.TimeIdentifier).GetTimePeriod());
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