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

                    await RemoveAsync(id);
                }
            }
        }

        public async Task DeleteFootnotes(Guid releaseId, List<Footnote> footnotes)
        {
            foreach (var footnote in footnotes)
            {
                await DeleteFootnote(releaseId, footnote.Id);
            }
        }
        
        public async Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId)
        {
            var otherFootnoteReferences = await _context
                .ReleaseFootnote
                .CountAsync(rf => rf.FootnoteId == footnoteId && rf.ReleaseId != releaseId);

            return otherFootnoteReferences == 0;
        }

        private async Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId)
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
    }
}