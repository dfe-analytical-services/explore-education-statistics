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

        public IEnumerable<Footnote> GetFootnotes(
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

        public async Task DeleteFootnotesForSubject(Guid releaseId, Guid subjectId)
        {
            var orphaned = GetFootnotesThatWillBeOrphaned(releaseId, subjectId);
            
            foreach (var footnote in orphaned)
            {
                await DeleteFootnote(releaseId, footnote.Id);
            }
        }

        public async Task DeleteFootnote(Guid releaseId, Guid id)
        {
            var footnote = _context.Footnote
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .Include(f => f.FilterItems)
                .Include(f => f.Indicators)
                .Include(f => f.Subjects).FirstOrDefault(f => f.Id == id);

            if (footnote != null)
            {
                await DeleteReleaseFootnoteLinkAsync(releaseId, footnote.Id);

                if (await IsFootnoteExclusiveToReleaseAsync(releaseId, footnote.Id))
                {
                    DeleteEntities(footnote.Subjects);
                    DeleteEntities(footnote.Filters);
                    DeleteEntities(footnote.FilterGroups);
                    DeleteEntities(footnote.FilterItems);
                    DeleteEntities(footnote.Indicators);

                    if (AllEntitiesMarkedForDeletion(footnote.Subjects))
                    {
                        await RemoveAsync(id);
                    }
                    else
                    {
                        await UpdateAsync(id);
                    }
                }
            }
        }

        public async Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId)
        {
            var otherFootnoteReferences = await _context
                .ReleaseFootnote
                .CountAsync(rf => rf.FootnoteId == footnoteId && rf.ReleaseId != releaseId);

            return otherFootnoteReferences == 0;
        }
        
        public IEnumerable<Footnote> GetFootnotes(Guid releaseId)
        {
            var subjectsIdsForRelease = _context.ReleaseSubject.Where(rs => rs.ReleaseId == releaseId).Select(rs => rs.SubjectId).ToList();

            return GetFootnotes(releaseId, subjectsIdsForRelease);
        }
        
        public List<Footnote> GetFootnotesThatWillBeOrphaned(Guid releaseId, Guid subjectId)
        {
            var footnotesLinkedToSubject = GetFootnotes(releaseId, new List<Guid>(){subjectId});
            
            return footnotesLinkedToSubject
                .Where(f => FootnoteLinkedToNoOtherSubject(subjectId, f))
                .ToList();
        }
        
        private IEnumerable<Footnote> GetFootnotes(Guid releaseId, List<Guid> subjects)
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
                    .Select(f => new Footnote()
                    {
                        Id = f.Id,
                        Content = f.Content,
                        Filters = f.Filters.Where(filterFootnote => subjects.Contains(filterFootnote.Filter.SubjectId)).ToList(),
                        Indicators = f.Indicators.Where(indicatorFootnote => subjects.Contains(indicatorFootnote.Indicator.IndicatorGroup.SubjectId)).ToList(),
                        Releases = f.Releases.Where(releaseFootnote => releaseFootnote.ReleaseId == releaseId).ToList(),
                        FilterGroups = f.FilterGroups.Where(filterGroupFootnote => subjects.Contains(filterGroupFootnote.FilterGroup.Filter.SubjectId)).ToList(),
                        FilterItems = f.FilterItems.Where(filterItemFootnote => subjects.Contains(filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId)).ToList(),
                        Subjects = f.Subjects.Where(subjectFootnote => subjects.Contains(subjectFootnote.SubjectId)).ToList()
                    }).AsEnumerable();
        }

        public async Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId)
        {
            var releaseFootnote = await _context
                .ReleaseFootnote
                .Where(rf => 
                    rf.ReleaseId == releaseId 
                    && rf.FootnoteId == footnoteId)
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
        
        private bool AllEntitiesMarkedForDeletion<T>(IEnumerable<T> entitiesToDelete)
        {
            foreach (var t in entitiesToDelete)
            {
                if (_context.Entry(t).State != EntityState.Deleted)
                {
                    return false;
                }
            }

            return true;
        }
        
        private static bool FootnoteLinkedToNoOtherSubject(Guid subjectId, Footnote f)
        {
            // if this Footnote is directly linked to any other Subjects than this one, then it's not just for
            // this Subject
            if (f.Subjects.Any(subject => subject.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter for any other Subject, then it's not just for
            // this Subject
            if (f.Filters.Any(filter => filter.Filter.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to an Indicator for any other Subject, then it's not just for
            // this Subject
            if (f.Indicators.Any(indicator => indicator.Indicator.IndicatorGroup.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter Group for any other Subject, then it's not just for
            // this Subject
            if (f.FilterGroups.Any(filterGroup => filterGroup.FilterGroup.Filter.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter Item for any other Subject, then it's not just for
            // this Subject
            if (f.FilterItems.Any(filterItem => filterItem.FilterItem.FilterGroup.Filter.SubjectId != subjectId))
            {
                return false;
            }

            return true;
        }
    }
}