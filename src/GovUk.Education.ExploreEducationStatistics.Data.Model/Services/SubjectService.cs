using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, Guid>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(StatisticsDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestPublishedRelease(Guid subjectId)
        {
            var publicationId = GetPublicationForSubjectAsync(subjectId).Result.Id;
            var latestRelease = _releaseService.GetLatestPublishedRelease(publicationId);

            if (!latestRelease.HasValue)
            {
                return false;
            }
            
            return _context
                .ReleaseSubject
                .Any(r => r.ReleaseId == latestRelease.Value && r.SubjectId == subjectId);
        }
        
        public bool Exists(Guid releaseId, string name)
        {
            return GetAsync(releaseId, name).Result != null;
        }

        public async Task<List<Footnote>> GetFootnotesOnlyForSubjectAsync(Guid subjectId)
        {
            var footnotesLinkedToSubject = await _context
                .Footnote
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
                    footnote.Subjects.Select(s => s.SubjectId).Contains(subjectId)
                    || footnote.Filters.Select(filterFootnote => filterFootnote.Filter.SubjectId).Contains(subjectId)
                    || footnote.FilterGroups.Select(filterGroupFootnote => filterGroupFootnote.FilterGroup.Filter.SubjectId).Contains(subjectId)
                    || footnote.FilterItems.Select(filterItemFootnote => filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId).Contains(subjectId)
                    || footnote.Indicators.Select(indicatorFootnote => indicatorFootnote.Indicator.IndicatorGroup.SubjectId).Contains(subjectId)
                )
                .ToListAsync();
             
            return footnotesLinkedToSubject
                .Where(f => FootnoteLinkedToNoOtherSubject(subjectId, f))
                .ToList();
        }

        public async Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId)
        {
            var releaseSubjectLinkToRemove = await _context
                .ReleaseSubject
                .FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.SubjectId == subjectId);

            if (releaseSubjectLinkToRemove == null)
            {
                return false;
            }
            
            var numberOfReleaseSubjectLinks = _context
                .ReleaseSubject
                .Count(s => s.SubjectId == subjectId);

            _context.ReleaseSubject.Remove(releaseSubjectLinkToRemove);
            
            if (numberOfReleaseSubjectLinks == 1)
            {
                var subject = await _context
                    .Subject
                    .FirstOrDefaultAsync(s => s.Id == subjectId);
            
                if (subject != null)
                {
                    var orphanFootnotes = await GetFootnotesOnlyForSubjectAsync(subjectId);

                    _context.Subject.Remove(subject);
                    _context.Footnote.RemoveRange(orphanFootnotes);
                }    
            }
        
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Subject> GetAsync(Guid releaseId, string name)
        {
            return await _context
                .ReleaseSubject
                .Include(r => r.Subject)
                .Where(r => r.ReleaseId == releaseId && r.Subject.Name == name)
                .Select(r => r.Subject)
                .FirstOrDefaultAsync();
        }

        public Task<Publication> GetPublicationForSubjectAsync(Guid subjectId)
        {
            return _context
                .ReleaseSubject
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .Where(r => r.SubjectId == subjectId)
                .Select(r => r.Release.Publication)
                .FirstAsync();
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

        public Task<List<Subject>> GetSubjectsForReleaseAsync(Guid releaseId)
        {
            return _context
                .ReleaseSubject
                .Include(s => s.Subject)
                .Where(s => s.ReleaseId == releaseId)
                .Select(s => s.Subject)
                .ToListAsync();
        } 
    }
}