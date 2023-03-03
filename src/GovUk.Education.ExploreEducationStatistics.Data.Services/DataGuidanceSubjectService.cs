#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class DataGuidanceSubjectService : IDataGuidanceSubjectService
    {
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly StatisticsDbContext _context;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly ITimePeriodService _timePeriodService;

        public DataGuidanceSubjectService(IIndicatorRepository indicatorRepository,
            StatisticsDbContext context,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            IReleaseDataFileRepository releaseDataFileRepository,
            IFootnoteRepository footnoteRepository,
            ITimePeriodService timePeriodService)
        {
            _indicatorRepository = indicatorRepository;
            _context = context;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _releaseDataFileRepository = releaseDataFileRepository;
            _footnoteRepository = footnoteRepository;
            _timePeriodService = timePeriodService;
        }

        public async Task<Either<ActionResult, List<DataGuidanceSubjectViewModel>>> GetSubjects(
            Guid releaseId,
            IEnumerable<Guid>? subjectIds = null)
        {
            return await _statisticsPersistenceHelper.CheckEntityExists<Release>(releaseId)
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
                        .ToListAsync();

                    var result = new List<DataGuidanceSubjectViewModel>();
                    await releaseSubjects
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async releaseSubject =>
                            {
                                result.Add(await BuildSubjectViewModel(releaseSubject));
                            });

                    return result
                        .OrderBy(viewModel => viewModel.Order)
                        .ThenBy(viewModel => viewModel.Name) // For subjects existing before ordering was added
                        .ToList();
                })
                // Currently we expect a failure checking the Release exists and succeed with an empty list.
                // StatisticsDb Releases are not always in sync with ContentDb Releases.
                // Until the first Subject is imported, no StatisticsDb Release exists.
                .OnFailureSucceedWith(_ => Task.FromResult(new List<DataGuidanceSubjectViewModel>()));
        }

        public async Task<Either<ActionResult, bool>> Validate(Guid releaseId)
        {
            var releaseSubjects = _context
                .ReleaseSubject
                .AsQueryable()
                .Where(rs => rs.ReleaseId == releaseId);

            return !await releaseSubjects.AnyAsync() || !await releaseSubjects.AnyAsync(
                rs => string.IsNullOrWhiteSpace(rs.DataGuidance));
        }

        public async Task<List<string>> GetGeographicLevels(Guid subjectId)
        {
            return await _context
                .Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == subjectId)
                .Select(observation => observation.Location.GeographicLevel.GetEnumLabel())
                .Distinct()
                .ToListAsync();
        }

        private List<LabelValue> GetVariables(Guid subjectId)
        {
            var filters = _context.Filter
                .Where(filter => filter.SubjectId == subjectId)
                .Select(filter =>
                    new LabelValue(
                        string.IsNullOrWhiteSpace(filter.Hint) ? filter.Label : $"{filter.Label} - {filter.Hint}",
                        filter.Name))
                .ToList();

            var indicators = _indicatorRepository.GetIndicators(subjectId)
                .Select(indicator => new LabelValue(indicator.Label, indicator.Name));

            return filters.Concat(indicators)
                .OrderBy(labelValue => labelValue.Value)
                .ToList();
        }

        private async Task<List<FootnoteViewModel>> GetFootnotes(Guid releaseId, Guid subjectId)
        {
            var footnotes = await _footnoteRepository.GetFootnotes(releaseId, subjectId);
            return footnotes
                .Select(footnote => new FootnoteViewModel(footnote.Id, footnote.Content))
                .ToList();
        }

        private async Task<DataGuidanceSubjectViewModel> BuildSubjectViewModel(ReleaseSubject releaseSubject)
        {
            var subject = releaseSubject.Subject;

            var releaseFile =
                await _releaseDataFileRepository.GetBySubject(
                    releaseSubject.ReleaseId,
                    releaseSubject.SubjectId);

            var geographicLevels = await GetGeographicLevels(subject.Id);
            var timePeriods = await _timePeriodService.GetTimePeriodLabels(subject.Id);
            var variables = GetVariables(subject.Id);
            var footnotes = await GetFootnotes(releaseSubject.ReleaseId, subject.Id);

            return new DataGuidanceSubjectViewModel
            {
                Id = subject.Id,
                Content = releaseSubject.DataGuidance ?? "",
                Filename = releaseFile.File.Filename,
                Order = releaseFile.Order,
                Name = releaseFile.Name ?? "",
                GeographicLevels = geographicLevels,
                TimePeriods = timePeriods,
                Variables = variables,
                Footnotes = footnotes
            };
        }
    }
}
