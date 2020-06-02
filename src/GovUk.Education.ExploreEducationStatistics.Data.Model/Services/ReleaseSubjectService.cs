using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseSubjectService : IReleaseSubjectService
    {
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFootnoteService _footnoteService;

        public ReleaseSubjectService(StatisticsDbContext statisticsDbContext, IFootnoteService footnoteService)
        {
            _statisticsDbContext = statisticsDbContext;
            _footnoteService = footnoteService;
        }

        public async Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId)
        {
            var releaseSubjectLinkToRemove = await _statisticsDbContext
                .ReleaseSubject
                .FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.SubjectId == subjectId);

            if (releaseSubjectLinkToRemove == null)
            {
                return false;
            }
            
            var numberOfReleaseSubjectLinks = _statisticsDbContext
                .ReleaseSubject
                .Count(s => s.SubjectId == subjectId);

            _statisticsDbContext.ReleaseSubject.Remove(releaseSubjectLinkToRemove);
            
            var footnotesForSubject = await GetFootnotesAsync(releaseId, subjectId);

            await _footnoteService.DeleteFootnotes(releaseId, footnotesForSubject);
            
            if (numberOfReleaseSubjectLinks == 1)
            {
                var subject = await _statisticsDbContext
                    .Subject
                    .FirstOrDefaultAsync(s => s.Id == subjectId);
            
                if (subject != null)
                {
                    _statisticsDbContext.Subject.Remove(subject);
                }    
            }
        
            await _statisticsDbContext.SaveChangesAsync();
            return true;
        }
        
        public async Task<List<Footnote>> GetFootnotesAsync(Guid releaseId, Guid subjectId)
        {
            var footnotesLinkedToSubject = await _statisticsDbContext
                .Footnote
                .Include(f => f.Releases)
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Include(f => f.FilterItems)
                .ThenInclude(fi => fi.FilterItem)
                .ThenInclude(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Include(f => f.Indicators)
                .ThenInclude(i => i.Indicator)
                .ThenInclude(i => i.IndicatorGroup)
                .Include(f => f.Subjects)
                .Where(footnote => 
                    footnote.Releases.Select(r => r.ReleaseId).Contains(releaseId) &&
                    (footnote.Subjects.Select(s => s.SubjectId).Contains(subjectId)
                    || footnote.Filters.Select(filterFootnote => filterFootnote.Filter.SubjectId).Contains(subjectId)
                    || footnote.FilterGroups.Select(filterGroupFootnote => filterGroupFootnote.FilterGroup.Filter.SubjectId).Contains(subjectId)
                    || footnote.FilterItems.Select(filterItemFootnote => filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId).Contains(subjectId)
                    || footnote.Indicators.Select(indicatorFootnote => indicatorFootnote.Indicator.IndicatorGroup.SubjectId).Contains(subjectId))
                )
                .ToListAsync();
             
            return footnotesLinkedToSubject
                .Where(f => FootnoteLinkedToNoOtherSubject(subjectId, f))
                .ToList();
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