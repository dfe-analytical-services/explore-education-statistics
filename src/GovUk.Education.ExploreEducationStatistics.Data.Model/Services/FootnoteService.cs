using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FootnoteService : AbstractRepository<Footnote, Guid>, IFootnoteService
    {
        public FootnoteService(StatisticsDbContext context,
            ILogger<FootnoteService> logger) : base(context, logger)
        {}

        public IEnumerable<Footnote> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicators)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem.Id).Distinct();

            var releaseIdParam = new SqlParameter("releaseId", releaseId);
            var subjectIdParam = new SqlParameter("subjectId", subjectId);
            var indicatorListParam = CreateIdListType("indicatorList", indicators);
            var filterItemListParam = CreateIdListType("filterItemList", filterItems);

            return _context
                .Footnote
                .FromSqlRaw(
                    "EXEC dbo.FilteredFootnotes " +
                    "@releaseId," +
                    "@subjectId," +
                    "@indicatorList," +
                    "@filterItemList", 
                    releaseIdParam,
                    subjectIdParam,
                    indicatorListParam,
                    filterItemListParam)
                .AsNoTracking();
        }

        public async Task DeleteAllFootnotesBySubject(Guid releaseId, Guid subjectId)
        {
            var footnotesLinkedToSubject = GetFootnotes(releaseId, subjectId);

            foreach (var footnote in footnotesLinkedToSubject)
            {
                var canRemoveReleaseFootnote = !IsFootnoteLinkedToAnotherSubjectForThisRelease(footnote.Id, releaseId, subjectId);
                var canRemoveFootnote = canRemoveReleaseFootnote && !IsFootnoteLinkedToAnotherSubject(footnote.Id, subjectId);

                await DeleteFootnote(releaseId, footnote, canRemoveReleaseFootnote, canRemoveFootnote);
            }
        }

        public async Task DeleteFootnote(Guid releaseId, Guid id)
        {
            var footnotes = GetFootnotes(releaseId).Where(f => f.Id == id);
            
            foreach (var footnote in footnotes)
            {
                await DeleteFootnote(releaseId, footnote, true, true);
            }
        }

        private async Task DeleteFootnote(Guid releaseId, Footnote footnote, bool canRemoveReleaseFootnote, bool canRemoveFootnote)
        {
            // Break link with the whole release if this footnote is not related to any other subject for the release
            // Else, as the ReleaseSubject link will be broken then footnote will not be visible.
            if (canRemoveReleaseFootnote)
            {
                await DeleteReleaseFootnoteLinkAsync(releaseId, footnote.Id);
            }

            if (await IsFootnoteExclusiveToReleaseAsync(releaseId, footnote.Id))
            {
                DeleteEntities(footnote.Subjects);
                DeleteEntities(footnote.Filters);
                DeleteEntities(footnote.FilterGroups);
                DeleteEntities(footnote.FilterItems);
                DeleteEntities(footnote.Indicators);
                
                if (canRemoveFootnote)
                {
                    await RemoveAsync(footnote.Id);
                }
            }
        }

        public async Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId)
        {
            var otherFootnoteReferences = await _context.ReleaseFootnote
                .CountAsync(rf => rf.FootnoteId == footnoteId && rf.ReleaseId != releaseId);

            return otherFootnoteReferences == 0;
        }
        
        public IEnumerable<Footnote> GetFootnotes(Guid releaseId)
        {
            var subjectsIdsForRelease = _context.ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId)
                .Select(rs => rs.SubjectId).ToArray();

            return GetFootnotes(releaseId, subjectsIdsForRelease);
        }

        public IEnumerable<Footnote> GetFootnotes(Guid releaseId, params Guid[] subjects)
        {
            return _context.Footnote
                    .Include(footnote => footnote.Filters)
                    .ThenInclude(filterFootnote => filterFootnote.Filter)
                    .Include(footnote => footnote.FilterGroups)
                    .ThenInclude(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                    .ThenInclude(filterGroup => filterGroup.Filter)
                    .Include(footnote => footnote.FilterItems)
                    .ThenInclude(filterItemFootnote => filterItemFootnote.FilterItem)
                    .ThenInclude(filterItem => filterItem.FilterGroup)
                    .ThenInclude(filterGroup => filterGroup.Filter)
                    .Include(footnote => footnote.Indicators)
                    .ThenInclude(indicatorFootnote => indicatorFootnote.Indicator)
                    .ThenInclude(indicator => indicator.IndicatorGroup)
                    .Include(footnote => footnote.Releases)
                    .Include(footnote => footnote.Subjects)
                    .Where(footnote => footnote.Releases.Any(releaseFootnote => releaseFootnote.ReleaseId == releaseId))
                    .Select(f => new Footnote
                    {
                        Id = f.Id,
                        Content = f.Content,
                        Filters = f.Filters.Where(filterFootnote => subjects.Contains(filterFootnote.Filter.SubjectId)).ToList(),
                        Indicators = f.Indicators.Where(indicatorFootnote => subjects.Contains(indicatorFootnote.Indicator.IndicatorGroup.SubjectId)).ToList(),
                        Releases = f.Releases.Where(releaseFootnote => releaseFootnote.ReleaseId == releaseId).ToList(),
                        FilterGroups = f.FilterGroups.Where(filterGroupFootnote => subjects.Contains(filterGroupFootnote.FilterGroup.Filter.SubjectId)).ToList(),
                        FilterItems = f.FilterItems.Where(filterItemFootnote => subjects.Contains(filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId)).ToList(),
                        Subjects = f.Subjects.Where(subjectFootnote => subjects.Contains(subjectFootnote.SubjectId)).ToList()
                    }).ToList()
                    .Where(f => (f.Filters != null && f.Filters.Any()) ||
                                  (f.Indicators != null && f.Indicators.Any()) ||
                                  (f.FilterGroups != null && f.FilterGroups.Any()) ||
                                  (f.FilterItems != null && f.FilterItems.Any()) ||
                                  (f.Subjects != null && f.Subjects.Any())
                    ).ToList();
        }

        public async Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId)
        {
            var releaseFootnote = await _context.ReleaseFootnote
                .Where(rf => rf.ReleaseId == releaseId && rf.FootnoteId == footnoteId)
                .FirstOrDefaultAsync();
            
            _context.ReleaseFootnote.Remove(releaseFootnote);
        }
        
        private void DeleteEntities<T>(IEnumerable<T> entitiesToDelete)
        {
            foreach (var t in entitiesToDelete)
            {
                _context.Entry(t).State = EntityState.Deleted;
            }
        }

        private bool IsFootnoteLinkedToAnotherSubject(Guid id, Guid subjectId)
        {
            var footnote = _context.Footnote
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .Include(f => f.FilterItems)
                .Include(f => f.Indicators)
                .ThenInclude(i => i.Indicator.IndicatorGroup)
                .Include(f => f.Subjects).Single(f => f.Id == id);

            return (footnote.Subjects.Any(subject => subject.SubjectId != subjectId) ||
                    footnote.Filters.Any(filter => filter.Filter.SubjectId != subjectId) ||
                    footnote.Indicators.Any(indicator => indicator.Indicator.IndicatorGroup.SubjectId != subjectId) ||
                    footnote.FilterGroups.Any(filterGroup => filterGroup.FilterGroup.Filter.SubjectId != subjectId) ||
                    footnote.FilterItems.Any(filterItem => filterItem.FilterItem.FilterGroup.Filter.SubjectId != subjectId));
        }
        
        private bool IsFootnoteLinkedToAnotherSubjectForThisRelease(Guid id, Guid releaseId, Guid subjectId)
        {
            var otherSubjectIds = _context.ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId && rs.SubjectId != subjectId)
                .ToList()
                .Select(rs => rs.SubjectId).ToList();
            
            var footnote = _context.Footnote
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .ThenInclude(f => f.FilterGroup.Filter)
                .Include(f => f.FilterItems)
                .ThenInclude(itemFootnote => itemFootnote.FilterItem.FilterGroup.Filter)
                .Include(f => f.Indicators)
                .ThenInclude(i => i.Indicator.IndicatorGroup)
                .Include(f => f.Subjects)
                .Single(f => f.Id == id);

            return footnote.Subjects.Any(subject => otherSubjectIds.Contains(subject.SubjectId)) ||
                    footnote.Filters.Any(filter => otherSubjectIds.Contains(filter.Filter.SubjectId)) ||
                    footnote.Indicators.Any(indicator => otherSubjectIds.Contains(indicator.Indicator.IndicatorGroup.SubjectId)) ||
                    footnote.FilterGroups.Any(filterGroup => otherSubjectIds.Contains(filterGroup.FilterGroup.Filter.SubjectId)) ||
                    footnote.FilterItems.Any(filterItem => otherSubjectIds.Contains(filterItem.FilterItem.FilterGroup.Filter.SubjectId));
        }
    }
}